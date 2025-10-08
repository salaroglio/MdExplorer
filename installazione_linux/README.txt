MdExplorer for Linux - Installation Guide
=========================================

Thank you for choosing MdExplorer!

CONTENTS OF THIS PACKAGE
------------------------
- mdexplorer-*.AppImage  : The MdExplorer application
- install.sh            : Installation script
- uninstall.sh          : Uninstallation script  
- README.txt            : This file

SYSTEM REQUIREMENTS
-------------------
MdExplorer requires the following to be installed on your system:

1. Java Runtime Environment (JRE) 8 or higher
   - Required for PlantUML diagram generation
   
2. Git
   - Required for version control features
   
3. Pandoc
   - Required for document export (PDF, Word)
   
4. LaTeX (optional)
   - For high-quality PDF export with custom styling

The installer will check for these dependencies and offer to install
them automatically on supported distributions.

SUPPORTED DISTRIBUTIONS
-----------------------
The installer supports automatic dependency installation on:
- Ubuntu / Debian / Linux Mint
- Fedora / RHEL / CentOS
- openSUSE / SLES
- Arch Linux / Manjaro

On other distributions, you'll need to install dependencies manually.

INSTALLATION
------------
1. Extract the archive to a temporary directory
2. Open a terminal in that directory
3. Make the installer executable:
   chmod +x install.sh
   
4. Run the installer:
   ./install.sh
   
5. Follow the on-screen prompts

The installer will:
- Check and install missing dependencies (with your permission)
- Install MdExplorer to /opt/mdexplorer/
- Create an application menu entry
- Create a desktop shortcut
- Add 'mdexplorer' command to your PATH

RUNNING MDEXPLORER
------------------
After installation, you can run MdExplorer in several ways:

1. Click the desktop icon
2. Find it in your Applications menu under Development or Office
3. Run 'mdexplorer' from the terminal
4. Right-click on .md files and select "Open with MdExplorer"

UNINSTALLATION
--------------
To remove MdExplorer:

1. Run the uninstaller:
   ./uninstall.sh
   
2. Confirm when prompted

Note: System dependencies (Java, Git, Pandoc) will NOT be removed
as they may be used by other applications.

TROUBLESHOOTING
---------------
If the AppImage doesn't run:
- Make sure it's executable: chmod +x mdexplorer-*.AppImage
- Try running from terminal to see error messages
- Check that all dependencies are installed

If PlantUML doesn't work:
- Verify Java is installed: java -version
- Should show version 8 or higher

If document export doesn't work:
- Verify Pandoc is installed: pandoc --version
- For PDF export, also check: pdflatex --version

MANUAL INSTALLATION
-------------------
If the installer doesn't work on your distribution:

1. Install dependencies manually:
   - Java JRE 8+
   - Git
   - Pandoc
   - LaTeX (optional, for PDF)

2. Copy files manually:
   sudo mkdir -p /opt/mdexplorer
   sudo cp mdexplorer-*.AppImage /opt/mdexplorer/mdexplorer.AppImage
   sudo chmod +x /opt/mdexplorer/mdexplorer.AppImage
   sudo ln -s /opt/mdexplorer/mdexplorer.AppImage /usr/local/bin/mdexplorer

3. Create desktop file at ~/.local/share/applications/mdexplorer.desktop

SUPPORT
-------
For help and support:
- GitHub: https://github.com/salaroglio/MdExplorer
- Email: salaroglio@hotmail.com

VERSION
-------
MdExplorer Version: 1.0.0
Installer Version: 1.0.0

LICENSE
-------
MdExplorer is licensed under ISC License.
See the application for full license details.