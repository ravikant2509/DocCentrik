DocCentrik: Centralized Document Discovery Tool 📂🔍
DocCentrik is a cutting-edge solution for document discovery, compliance, and centralized storage, designed to simplify your workflows and help meet GDPR and internal audit requirements. With regex and keyword-based searches, multi-format file support, and secure SFTP uploads, DocCentrik ensures effortless management of your critical files.

🌟 Features
Smart Search: Find files using keywords or regex patterns.
Multi-Format Support: Process .txt, .csv, .pdf, .docx, .xlsx, .pptx, and images.
GDPR Compliance: Identify sensitive data for audits and regulatory needs.
Centralized Storage: Securely upload files to an SFTP server.
Daily Logs and Reports: Automatically generate logs for full transparency.
OCR Integration: Extract text from images and scanned files with Tesseract.
📂 Project Structure


DocCentrik/
│
├── Config/                # Configuration management
│   └── Config.cs          # Defines application settings and models
│
├── logs/                  # Logs generated by the application
│   ├── DocCentrikLog.csv                 # Default log file
│   ├── DocCentrikLog_YYYY-MM-DD.csv      # Daily logs
│   ├── DocCentrikMatchReport.csv         # Match report log file
│   └── DocCentrikMatchReport_YYYY-MM-DD.csv # Daily match reports
│
├── Services/              # Core services
│   ├── FileProcessor.cs   # Handles file scanning and content extraction
│   └── SftpUploader.cs    # Manages secure file uploads via SFTP
│
├── tessdata/              # OCR Data
│   └── eng.traineddata    # Tesseract OCR language file
│
├── Utils/                 # Utility classes
│   └── Logger.cs          # Logs processing and match results
│
├── x64/                   # x64 dependencies (Tesseract native binaries)
├── x86/                   # x86 dependencies (Tesseract native binaries)
├── config.json            # User-configurable settings file
├── Program.cs             # Main application entry point
├── LICENSE                # License file
├── README.md              # Project documentation
└── DocCentrik_User_Guide.pdf # User Guide (detailed instructions)


📋 System Requirements
Operating System: Windows 10 or later.
.NET Framework: Version 4.8 or later.
Target Framework: .NET 8.0.
Tesseract OCR Data: Download from Tesseract GitHub.
Hardware: Minimum 8GB RAM, 100GB free disk space.
SFTP Server: Valid credentials and access.
🚀 Getting Started
1. Clone the Repository
bash

git clone https://github.com/ravikant2509/DocCentrik.git
cd DocCentrik
2. Install Dependencies
Ensure the .NET SDK and required NuGet packages are installed:



dotnet restore
3. Configure Settings
Edit the config.json file to customize the application:

json

{
  "FolderPath": "C:\\Users\\YourName\\Documents",
  "Keywords": ["Account", "Customer", "Loan"],
  "RegexPatterns": [
    {
      "Pattern": "[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}",
      "Description": "Email addresses"
    }
  ],
  "SearchMode": "both",
  "SupportedExtensions": [".txt", ".csv", ".pdf", ".docx", ".xlsx", ".pptx"],
  "OutputLogPath": "C:\\Logs",
  "MatchReportPath": "C:\\Logs",
  "TesseractDataPath": "C:\\Tesseract\\tessdata",
  "EnableSftp": true,
  "SftpServer": {
    "Host": "sftp.example.com",
    "Port": 22,
    "Username": "username",
    "Password": "password"
  }
}
4. Build and Run


dotnet build
dotnet run
🛠 How It Works
Search Directories: Locate files matching the specified extensions.
Extract Content: Use keywords, regex patterns, and OCR to extract insights.
Log Activity: Generate daily logs for all processing and matches.
Centralize Files: Upload matched files to a secure SFTP server.
📄 Documentation
Refer to the User Guide for detailed instructions on setup, configuration, and usage.

🤝 Contributing
Contributions are welcome! To get started:

Fork this repository.
Create a feature branch:


git checkout -b feature/YourFeatureName
Commit your changes:
bash

git commit -m "Add feature: YourFeatureName"
Push the branch:


git push origin feature/YourFeatureName
Open a pull request.
📄 License
This project is licensed under the MIT License. See the LICENSE file for details.

🌟 Support
For issues, questions, or feature requests, open a discussion or create an issue on GitHub.
Created by Ravikant Sharma – https://github.com/ravikant2509.