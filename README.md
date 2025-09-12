# Bite Board
Bite Board related repositories.

## ADD MIGRATION
### Starup Project: BiteBoard.Api OR BiteBoard.Data
### Default Project: BiteBoard.Data

Add-Migration InitialIdentityModels -Context IdentityContext
Add-Migration InitialModels -Context ApplicationDbContext

## UPDATE DATABASE
### Starup Project: BiteBoard.Api OR BiteBoard.Data
### Default Project: BiteBoard.Data

Update-Database -Context IdentityContext
Update-Database -Context ApplicationDbContext

## DROP DATABASE
### Starup Project: BiteBoard.Api OR BiteBoard.Data
### Default Project: BiteBoard.Data

Drop-Database -Context IdentityContext
Drop-Database -Context ApplicationDbContext

## REMOVE MIGRATION
### Starup Project: BiteBoard.Api OR BiteBoard.Data
### Default Project: BiteBoard.Data

Remove-Migration -Context IdentityContext
Remove-Migration -Context ApplicationDbContext
