## Description

Please provide a brief description of the changes in this pull request.

## Type of Change

Please check the relevant option(s):

- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update
- [ ] Code refactoring (no functional changes)
- [ ] Performance improvement
- [ ] Test coverage improvement

## Related Issues

Closes #(issue number)

## Testing

Please describe the tests you ran to verify your changes:

- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed
- [ ] Tested on multiple platforms (Windows, macOS, Linux)
- [ ] Tested in Electron app
- [ ] Tested in web browser

### Test Configuration

- **OS**: [e.g. Windows 11, Ubuntu 22.04]
- **.NET SDK**: [e.g. 8.0.100]
- **Node.js**: [e.g. 14.21.3]

## Checklist

Before submitting this PR, please ensure:

- [ ] My code follows the project's code style and conventions
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings or errors
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
- [ ] Any dependent changes have been merged and published

## Cross-Platform Compatibility

**IMPORTANT**: MdExplorer must work on Windows, macOS, and Linux.

- [ ] I have used `Path.Combine()` for file system paths (not string concatenation)
- [ ] I have used forward slashes `/` for Markdown/HTML/Pandoc paths
- [ ] I have tested or verified cross-platform compatibility
- [ ] No platform-specific code introduced (or properly abstracted if necessary)

## Database Changes

If this PR includes database changes:

- [ ] FluentMigrator migration created with proper naming (`M{Year}_{Month}_{Day}_{Number}`)
- [ ] Migration tested with empty database
- [ ] Migration tested with existing data
- [ ] NHibernate mappings updated if needed
- [ ] Followed patterns in [workflow-sviluppo-dati-persistenti.md](../workflow-sviluppo-dati-persistenti.md)

## Screenshots (if applicable)

Add screenshots to help reviewers understand the changes.

## Additional Notes

Any additional information that reviewers should know.
