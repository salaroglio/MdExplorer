# Security Policy

## Supported Versions

Currently supported versions of MdExplorer:

| Version | Supported          |
| ------- | ------------------ |
| 2025.x  | :white_check_mark: |
| < 2025  | :x:                |

## Reporting a Vulnerability

**IMPORTANT: Please do NOT open a public GitHub issue for security vulnerabilities.**

To report a security vulnerability, please email: **security@mdexplorer.net**

Include the following information:
- Clear description of the vulnerability
- Steps to reproduce the issue
- Potential impact and severity assessment
- Suggested fix or mitigation (if available)
- Your contact information for follow-up

### What to Expect

- **Initial Response**: We will acknowledge receipt of your report within 48 hours
- **Updates**: We will provide status updates every 7 days until the issue is resolved
- **Resolution**: We aim to release a fix within 30 days for critical vulnerabilities
- **Credit**: We will credit you in the security advisory (unless you prefer to remain anonymous)

## Security Measures in MdExplorer

The project implements several security measures:

- **Git Operations**: Uses LibGit2Sharp library (no shell command injection risks)
- **Credential Storage**: Leverages OS-native credential stores
- **Input Validation**: All user inputs are validated and sanitized
- **Database Access**: Uses NHibernate ORM with parameterized queries
- **File Operations**: Path validation to prevent directory traversal attacks
- **Dependencies**: Regular updates and security scanning

## Security Best Practices for Users

- Keep MdExplorer updated to the latest version
- Use SSH keys or OS credential managers for Git authentication
- Avoid storing sensitive data in markdown files committed to public repositories
- Review permissions when using AI features with sensitive documents
- Enable auto-update feature for timely security patches

## Disclosure Policy

- Security vulnerabilities will be disclosed publicly only after a fix is available
- We follow a coordinated disclosure process
- Critical vulnerabilities will be announced via GitHub Security Advisories
- All users will be notified through release notes and project website

Thank you for helping keep MdExplorer and its users safe!
