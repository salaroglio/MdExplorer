using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Lettore, in sola lettura, del «file composto» di Windows (OLE, [MS-CFB]): il contenitore in cui Word
    /// tiene un allegato incorporato (<c>word/embeddings/oleObjectN.bin</c>). Dentro ci sono dei flussi con un
    /// nome; qui se ne chiede uno per nome, a qualunque livello stia.
    /// <para>
    /// Si legge solo quello che serve a trovare un flusso: intestazione, tabella dei settori (FAT, con i suoi
    /// settori DIFAT), elenco delle voci, e il «mini flusso» dove stanno i flussi piccoli.
    /// </para>
    /// </summary>
    public sealed class CompoundFile
    {
        private const uint EndOfChain = 0xFFFFFFFE;
        private const uint FreeSector = 0xFFFFFFFF;
        private static readonly byte[] Signature = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };

        private readonly byte[] _file;
        private readonly int _sectorSize;
        private readonly int _miniSectorSize;
        private readonly uint _miniCutoff;
        private readonly uint[] _fat;
        private readonly uint[] _miniFat;
        private readonly byte[] _miniStream;
        private readonly List<(string Name, uint Start, long Size)> _streams = new List<(string Name, uint Start, long Size)>();

        public static bool LooksLikeOne(byte[] file)
        {
            if (file == null || file.Length < 512)
            {
                return false;
            }
            for (var i = 0; i < Signature.Length; i++)
            {
                if (file[i] != Signature[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <exception cref="InvalidDataException">Il file non è un file composto, o è rovinato: dice dove.</exception>
        public CompoundFile(byte[] file)
        {
            if (!LooksLikeOne(file))
            {
                throw new InvalidDataException("non è un file composto OLE (manca la firma)");
            }
            _file = file;
            _sectorSize = 1 << BitConverter.ToUInt16(file, 0x1E);
            _miniSectorSize = 1 << BitConverter.ToUInt16(file, 0x20);
            if (_sectorSize != 512 && _sectorSize != 4096)
            {
                throw new InvalidDataException($"settori di {_sectorSize} byte: non previsti");
            }
            var firstDirectory = BitConverter.ToUInt32(file, 0x30);
            _miniCutoff = BitConverter.ToUInt32(file, 0x38);
            var firstMiniFat = BitConverter.ToUInt32(file, 0x3C);
            var firstDifat = BitConverter.ToUInt32(file, 0x44);

            // Dove stanno i settori della FAT: 109 posizioni nell'intestazione, le altre in una catena di settori DIFAT.
            var fatSectors = new List<uint>();
            for (var i = 0; i < 109; i++)
            {
                var sector = BitConverter.ToUInt32(file, 0x4C + i * 4);
                if (sector < EndOfChain)
                {
                    fatSectors.Add(sector);
                }
            }
            var perSector = _sectorSize / 4;
            var guard = 0;
            for (var difat = firstDifat; difat < EndOfChain; )
            {
                var offset = Offset(difat);
                for (var i = 0; i < perSector - 1; i++)
                {
                    var sector = BitConverter.ToUInt32(file, offset + i * 4);
                    if (sector < EndOfChain)
                    {
                        fatSectors.Add(sector);
                    }
                }
                difat = BitConverter.ToUInt32(file, offset + (perSector - 1) * 4);
                if (++guard > file.Length / _sectorSize)
                {
                    throw new InvalidDataException("la catena DIFAT gira su se stessa");
                }
            }

            _fat = new uint[fatSectors.Count * perSector];
            for (var s = 0; s < fatSectors.Count; s++)
            {
                var offset = Offset(fatSectors[s]);
                for (var i = 0; i < perSector; i++)
                {
                    _fat[s * perSector + i] = BitConverter.ToUInt32(file, offset + i * 4);
                }
            }

            var directory = ReadChain(firstDirectory, long.MaxValue);
            uint rootStart = EndOfChain;
            long rootSize = 0;
            for (var entry = 0; entry + 128 <= directory.Length; entry += 128)
            {
                var type = directory[entry + 0x42];
                var nameBytes = BitConverter.ToUInt16(directory, entry + 0x40);
                if (type == 0 || nameBytes < 2 || nameBytes > 64)
                {
                    continue;
                }
                var name = Encoding.Unicode.GetString(directory, entry, nameBytes - 2);
                var start = BitConverter.ToUInt32(directory, entry + 0x74);
                // Nei file con settori da 512 la metà alta della misura non è affidabile.
                long size = _sectorSize == 512 ? BitConverter.ToUInt32(directory, entry + 0x78) : BitConverter.ToInt64(directory, entry + 0x78);
                if (type == 5)
                {
                    rootStart = start;
                    rootSize = size;
                }
                else if (type == 2)
                {
                    _streams.Add((name, start, size));
                }
            }

            _miniStream = rootStart < EndOfChain ? ReadChain(rootStart, rootSize) : new byte[0];
            var miniFatBytes = firstMiniFat < EndOfChain ? ReadChain(firstMiniFat, long.MaxValue) : new byte[0];
            _miniFat = new uint[miniFatBytes.Length / 4];
            for (var i = 0; i < _miniFat.Length; i++)
            {
                _miniFat[i] = BitConverter.ToUInt32(miniFatBytes, i * 4);
            }
        }

        public IEnumerable<string> StreamNames
        {
            get
            {
                foreach (var stream in _streams)
                {
                    yield return stream.Name;
                }
            }
        }

        /// <summary>Il contenuto del flusso con quel nome; null se non c'è.</summary>
        public byte[] Stream(string name)
        {
            foreach (var stream in _streams)
            {
                if (string.Equals(stream.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return stream.Size < _miniCutoff ? ReadMiniChain(stream.Start, stream.Size) : ReadChain(stream.Start, stream.Size);
                }
            }
            return null;
        }

        private int Offset(uint sector)
        {
            var offset = ((long)sector + 1) * _sectorSize;
            if (offset + _sectorSize > _file.Length)
            {
                // L'ultimo settore di un file può essere tronco: lo si accetta se almeno comincia dentro il file.
                if (offset >= _file.Length)
                {
                    throw new InvalidDataException($"il settore {sector} sta oltre la fine del file");
                }
            }
            return (int)offset;
        }

        private byte[] ReadChain(uint start, long size)
        {
            using (var buffer = new MemoryStream())
            {
                var guard = 0;
                for (var sector = start; sector < EndOfChain && buffer.Length < size; )
                {
                    var offset = Offset(sector);
                    buffer.Write(_file, offset, Math.Min(_sectorSize, _file.Length - offset));
                    if (sector >= _fat.Length)
                    {
                        throw new InvalidDataException($"il settore {sector} non è nella tabella dei settori");
                    }
                    sector = _fat[sector];
                    if (sector == FreeSector || ++guard > _fat.Length)
                    {
                        throw new InvalidDataException("una catena di settori è rotta");
                    }
                }
                return Cut(buffer.ToArray(), size);
            }
        }

        private byte[] ReadMiniChain(uint start, long size)
        {
            using (var buffer = new MemoryStream())
            {
                var guard = 0;
                for (var sector = start; sector < EndOfChain && buffer.Length < size; )
                {
                    var offset = (long)sector * _miniSectorSize;
                    if (offset + _miniSectorSize > _miniStream.Length || sector >= _miniFat.Length)
                    {
                        throw new InvalidDataException($"il mini settore {sector} sta fuori dal mini flusso");
                    }
                    buffer.Write(_miniStream, (int)offset, _miniSectorSize);
                    sector = _miniFat[sector];
                    if (sector == FreeSector || ++guard > _miniFat.Length)
                    {
                        throw new InvalidDataException("una catena di mini settori è rotta");
                    }
                }
                return Cut(buffer.ToArray(), size);
            }
        }

        private static byte[] Cut(byte[] bytes, long size)
        {
            if (size == long.MaxValue || size >= bytes.Length)
            {
                if (size != long.MaxValue && size > bytes.Length)
                {
                    throw new InvalidDataException($"il flusso dichiara {size} byte ma ne ha {bytes.Length}");
                }
                return bytes;
            }
            var cut = new byte[size];
            Array.Copy(bytes, cut, size);
            return cut;
        }
    }
}
