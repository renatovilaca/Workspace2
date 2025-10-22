# Dashboard Usage Guide

## Overview
The Robot.ED.FacebookConnector.Dashboard application is now fully functional with Identity authentication and user management capabilities.

## Getting Started

### 1. Starting the Application

#### Using Docker Compose (Recommended)
```bash
# Start PostgreSQL database
docker compose up postgres -d

# Run the Dashboard application
cd Robot.ED.FacebookConnector.Dashboard
dotnet run
```

The application will be available at:
- HTTP: http://localhost:7000
- HTTPS: https://localhost:7001

### 2. Default Admin Account

On first startup, the application automatically creates an admin user with the following credentials:

- **Email:** admin@localhost.local
- **Password:** Admin@123

**Important:** Change this password after first login!

### 3. Database Initialization

The application automatically:
- Applies Entity Framework migrations to create all necessary tables
- Creates "Admin" and "User" roles
- Seeds the default admin user
- Creates sample data if the database is empty

## Features

### Authentication
- **Login:** Navigate to `/Identity/Account/Login` or click "Login" in the navigation bar
- **Register:** New users can register at `/Identity/Account/Register`
- **Logout:** Click "Logout" in the navigation bar when logged in
- **Profile Management:** Access your profile at `/Identity/Account/Manage`

### User Management (Admin Only)
Access the user management interface at `/Users` or click "Users" in the navigation bar.

#### Available Operations:
- **List Users:** View all users with their roles and email confirmation status
- **Create User:** Add new users with specific roles
- **Edit User:** Update user information, assign roles, reset passwords
- **Delete User:** Remove users from the system (cannot delete your own account)

## Database Structure

### Identity Tables (created automatically)
- AspNetUsers
- AspNetRoles  
- AspNetUserRoles
- AspNetUserClaims
- AspNetRoleClaims
- AspNetUserLogins
- AspNetUserTokens

### Application Tables
- Queues, QueueData, QueueResults, QueueResultAttachment, QueueResultMessage, QueueTag
- Robots
- token (authentication tokens)
- user (API users)
- user_action_log (audit trail)

## Configuration

### Connection String
Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=robotfacebookconnector;Username=postgres;Password=postgres"
}
```

### Password Requirements
Default password requirements (configured in Program.cs):
- Minimum length: 6 characters
- Require digit: Yes
- Require uppercase: Yes
- Require lowercase: Yes
- Require non-alphanumeric: No

## Troubleshooting

### Database Connection Issues
- Ensure PostgreSQL is running: `docker ps | grep postgres`
- Check connection string in appsettings.json
- Verify database exists and credentials are correct

### Login Issues
- Use the full email address (admin@localhost.local) not just username
- Password is case-sensitive: Admin@123
- Check that the AspNetUsers table contains the admin user

### Migration Issues
- Migrations are applied automatically on startup
- Check application logs for migration errors
- Manually run migrations: `dotnet ef database update --context ApplicationDbContext`

## Development

### Adding New Migrations
```bash
# For Identity context
dotnet ef migrations add MigrationName --context ApplicationDbContext

# For application context
dotnet ef migrations add MigrationName --context AppDbContext
```

### Running Tests
```bash
dotnet test
```

## Security Notes
- Always use HTTPS in production
- Change the default admin password immediately
- Configure proper CORS policies
- Enable email confirmation in production
- Use strong passwords
- Implement rate limiting for login attempts
- Configure proper logging and monitoring

## Support
For issues or questions, please refer to the project documentation or contact the development team.
