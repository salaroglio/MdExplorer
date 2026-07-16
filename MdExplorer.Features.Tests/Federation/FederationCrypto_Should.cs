using System;
using System.Linq;
using MdExplorer.Features.Federation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Federation
{
    [TestClass]
    public class FederationCrypto_Should
    {
        private const string Secret = "s3cr3t-room-key-shared-via-git";
        private const string Room = "a1b2c3d4e5f6a7b8";

        [TestMethod]
        public void Round_trip_a_message()
        {
            var envelope = FederationCrypto.Encrypt(Secret, Room, "ciao mondo federato");
            StringAssert.StartsWith(envelope, "mdfed.v1.");
            Assert.AreEqual("ciao mondo federato", FederationCrypto.Decrypt(Secret, Room, envelope));
        }

        [TestMethod]
        public void Round_trip_empty_and_unicode()
        {
            Assert.AreEqual("", FederationCrypto.Decrypt(Secret, Room, FederationCrypto.Encrypt(Secret, Room, "")));
            var s = "àèìòù — 日本語 — 🔐";
            Assert.AreEqual(s, FederationCrypto.Decrypt(Secret, Room, FederationCrypto.Encrypt(Secret, Room, s)));
        }

        [TestMethod]
        public void Use_a_random_nonce_so_two_envelopes_differ()
        {
            var a = FederationCrypto.Encrypt(Secret, Room, "stesso testo");
            var b = FederationCrypto.Encrypt(Secret, Room, "stesso testo");
            Assert.AreNotEqual(a, b, "il nonce casuale rende ogni busta diversa");
            // ...ma entrambe decifrano allo stesso plaintext.
            Assert.AreEqual("stesso testo", FederationCrypto.Decrypt(Secret, Room, a));
            Assert.AreEqual("stesso testo", FederationCrypto.Decrypt(Secret, Room, b));
        }

        [TestMethod]
        public void Derive_the_same_key_across_cities()
        {
            // Interop: stesso secret + roomId → stessa chiave (ogni città apre le buste altrui).
            var k1 = FederationCrypto.DeriveKey(Secret, Room);
            var k2 = FederationCrypto.DeriveKey(Secret, Room);
            CollectionAssert.AreEqual(k1, k2);
            Assert.AreEqual(32, k1.Length);
        }

        [TestMethod]
        public void Derive_a_different_key_for_a_different_room()
        {
            var k1 = FederationCrypto.DeriveKey(Secret, Room);
            var k2 = FederationCrypto.DeriveKey(Secret, "ffffffffffffffff");
            Assert.IsFalse(k1.SequenceEqual(k2), "roomId diverso (salt) → chiave diversa");
        }

        [TestMethod]
        public void Fail_loud_on_a_tampered_envelope()
        {
            var envelope = FederationCrypto.Encrypt(Secret, Room, "integro");
            // Ribalta un carattere del base64 (dentro il ciphertext/tag).
            var chars = envelope.ToCharArray();
            var i = chars.Length - 3;
            chars[i] = chars[i] == 'A' ? 'B' : 'A';
            var tampered = new string(chars);

            Assert.ThrowsException<FederationCryptoException>(
                () => FederationCrypto.Decrypt(Secret, Room, tampered));
        }

        [TestMethod]
        public void Fail_loud_with_the_wrong_secret()
        {
            var envelope = FederationCrypto.Encrypt(Secret, Room, "segreto");
            Assert.ThrowsException<FederationCryptoException>(
                () => FederationCrypto.Decrypt("secret-sbagliato", Room, envelope));
        }

        [TestMethod]
        public void Fail_loud_with_the_wrong_room()
        {
            var envelope = FederationCrypto.Encrypt(Secret, Room, "segreto");
            Assert.ThrowsException<FederationCryptoException>(
                () => FederationCrypto.Decrypt(Secret, "0000000000000000", envelope));
        }

        [TestMethod]
        public void Fail_loud_on_an_unknown_version()
        {
            Assert.ThrowsException<FederationCryptoException>(
                () => FederationCrypto.Decrypt(Secret, Room, "mdfed.v2.AAAA"));
            Assert.ThrowsException<FederationCryptoException>(
                () => FederationCrypto.Decrypt(Secret, Room, "not-an-envelope"));
        }

        [TestMethod]
        public void Fail_loud_on_malformed_base64_or_short_envelope()
        {
            Assert.ThrowsException<FederationCryptoException>(
                () => FederationCrypto.Decrypt(Secret, Room, "mdfed.v1.!!!not-base64!!!"));
            Assert.ThrowsException<FederationCryptoException>(
                () => FederationCrypto.Decrypt(Secret, Room, "mdfed.v1.AAAA")); // troppo corta per nonce+tag
        }

        [TestMethod]
        public void Reject_an_empty_secret_when_deriving()
        {
            Assert.ThrowsException<ArgumentException>(() => FederationCrypto.DeriveKey("", Room));
            Assert.ThrowsException<ArgumentException>(() => FederationCrypto.DeriveKey(Secret, ""));
        }
    }
}
