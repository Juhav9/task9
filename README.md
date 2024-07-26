[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-24ddc0f5d75046c5622901739e7c5dd533143b0c8e959d652212380cedb1ea36.svg)](https://classroom.github.com/a/2v6T0LFl)
# Task 09: ASP.NET MVC app with Authentication

<img alt="points bar" align="right" height="36" src="../../blob/badges/.github/badges/points-bar.svg" />

![GitHub Classroom Workflow](../../workflows/GitHub%20Classroom%20Workflow/badge.svg)

***

## Student info

> Write your name, your estimation of how many points you will get, and an estimate of how long it took to make the answer

- Student name: 
- Estimated points: 
- Estimated time (hours): 

***

## Purpose of this task

The purposes of this task are:

- to learn basic authentication scenario with ASP.NET MVC app
- to learn about Areas in ASP.NET MVC app
- to handle logged-in user's data properly
- to learn about ASP.NET Individual User Account authentication
- to learn about Authentication (who the user is) and Authorization (what the user is allowed to do)

## Material for the task

> **Following material will help with the task.**

It is recommended that you will check the material before start coding.

1. [Get started with ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc?view=aspnetcore-6.0)
2. [ASP.NET Core security topics](https://learn.microsoft.com/en-us/aspnet/core/security)
3. [Overview of ASP.NET Core authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication)
4. [Articles based on ASP.NET Core projects created with individual user accounts](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/individual)
5. [Introduction to authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)
6. [Role-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)
7. [Identity model customization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model)
8. [Areas in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/areas)
9. [Creating and configuring a model](https://learn.microsoft.com/en-us/ef/core/modeling/)
10. [One-to-many relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many)
11. [Cascade Delete](https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete)

## The Task

Create an ASP.NET MVC web application that uses Individual user accounts (i.e. the user accounts are handled and persisted in the app's database). The src folder contains a project named DeviceManager that was created with dotnet CLI command `dotnet new mvc -au Individual`. The created template has been changed to enable testing and to show the three (3) users in the database at Home/Index view. The app.db database contains the three users and the required roles for the app. The DB also has some devices linked to the users.

Complete the app to support functionality to users in different roles. There are four types of users:
1. Anonymous user (i.e. not logged-in)
2. Authenticated user (i.e. logged-in, a normal user)
3. Manager user, belongs to `Managers` group
4. Admin user, belongs to `Admins` group

> Note that terms `role` and `group` are used interchangeably and, they mean the same thing. Being in a `role` is usually implemented by `group` membership (i.e. belonging to a group indicates that a user is in a role).

### Anonymous user

Anonymous users can:
- access the app's main page (Home/Index).
- register.
- login.

### Authenticated user

Every logged-in user is by default an authenticated user (i.e. a normal user). An authenticated user can CRUD (create, read, update, and delete) their own devices. The normal user must not be able to access other users' devices. This logic resembles resource-based access. If the logged-in user does not have rights to access the device then he is redirected to the default error page in an MVC application (i.e. to HomeController's Error action).

In this task simplified resource-based access is used. You will need to obtain the logged-in user's `user id` and use that with the database query to ensure that the user has the right to access the device. 

In ASP.NET applications, when authorization is enforced, you have access to the `User` property which is of type [ClaimsPrincipal](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal?view=net-6.0). The User object will have some claims and the user's id value is in claim type `ClaimTypes.NameIdentifier`. Use that to get the logged-in user's user id.

> Note! The Microsoft Learn documentation describes another way to do resource-based authorization but for this task, it is more complicated than that what is needed for the task. You can read the other way to do it in the document [Resource-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-6.0).  

### Manager user

Manager user belongs to `Managers` group and is also an authenticated user. In addition to the actions that an authenticated user can do the managers can manage (i.e. CRUD) other users' devices.

Functionality for the manager users is implemented in an Area named `Management` which is accessible from url `https://localhost:<port>/management`. Only users in role `Managers` can access the Management area.

### Admin user

Admin user belongs to `Admins` group and is also an authenticated user. In addition to the actions that an authenticated user can do the admins can delete user accounts and manage members (i.e. add or remove) of the Managers group. Functionality for the admin users is implemented in an Area named `Administration` which is accessible from url `https://localhost:<port>/administration`. Only users in role `Admins` can access the Administration area.

### Other requirements

All of the code must use properly configured `[Authorize]` attributes to allow or deny role based access.

Implement `DeviceManagerAppUser` class in `DeviceManager.Data` namespace in Data folder. That class must be configured to be used as the default identity type instead of the template's default `IdentityUser` class. The app user has an additional property named `Devices` for the user's devices and a property named `UserRoles` for the user's roles. Use proper types for the properties. Other properties are inherited from the IdentityUser class. Change the template's `ApplicationDbContext` class to use the created DeviceManagerAppUser class as the IdentityDbContext type. Note that the default ASP.NET Identity has many-to-many relationship between user and role tables (i.e. a user can have many roles and a role can have many users). **Remember** to change the type for the `SignInManager<T>` and the `UserManager<T>` in Views/Shared/_LoginPartial.cshtml file.

Implement `Device` class in `DeviceManager.Data` namespace in Data folder. The Device class contains all information about a device and models the Devices table in the DB. The Device class also has a virtual property for the `DeviceManagerAppUser` class (i.e. the user object who's device the class' instance is). Add the devices table to the ApplicationDbContext as a DbSet named `Devices` and model the Device entity so that the database's restrictions for data fields and relations are implemented. Model the restrictions with attributes.

The Device class could be used as a view model for the views that interacts with device data but when the Device class described above is properly configured for the database then it's usage as a model class for views is difficult. **It is recommended** to create another class to be used as a view model for the device data and handle data mapping between classes in the controller code. So create a `DeviceViewModel` class in `DeviceManager.Models` namespace in Models folder. Implement the properties that are needed to handle all CRUD operations on a device. Use the same property names for the implemented properties as the Device class has.

The Devices table is modelled from the following T-SQL (a MS SQL flavor of SQL-language) and transformed to fit SQLite database. Implement the `Device` class to match a row in the Devices table. Notice that there is a one-to-many relationship between AspNetUsers table and the Devices table (i.e. a user may have many devices but one device belongs to only one user).

```sql
CREATE TABLE [Devices] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(max),
    [DateAdded] datetime(7),
    CONSTRAINT [PK_Devices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Devices_AspNetUsers_Id] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
```

> **Note!** Do not modify the NuGet package references (do not add, do not remove and do not update).

### Evaluation points

1. Configuration

    The `Device` class is properly implemented as described in The Task chapter. 
    The `DeviceViewModel` class is properly implemented as described in The Task chapter.
    The `DeviceManagerAppUser` class is properly implemented as described in The Task chapter.
    The `ApplicationDbContext` class with necessary other classes are properly implemented as described in The Task chapter.
    Authentication and authorization is properly configured with roles support in Program.cs file.

2. Management area

    All the content in the Management area is accessible only to users who are in `Managers` role. The area's main page (/management) must be handled in the area's HomeController's Index action method. The rendered view contains a table element with all the users (`DeviceManagerAppUser` class instances) in the application's database. The table has header row and columns for a `user's id` and `email` fields and a column for link element (a tag) to the user's devices in the Management area (i.e. the `Index` action of `UserDevicesController`). Use Authorize attribute with proper parameters to all controller classes in the Management area.

    The manager's controls for a user's devices is modelled in `UserDevicesController`. The controller has Index, Details, Create, Edit and Delete actions for the selected user's devices. The class is in namespace `DeviceManager.Areas.Management.Controllers`. The selected user's id is passed to action methods in query parameter named `userId` (i.e. the url `../userdevices/details/X?userId=Y` must render user's with id `Y` device's with id `X` details). Use `DeviceViewModel` or `collection of DeviceViewModels` as the model class to the views.

    > Note that you may need `_ViewImports.cshtml` and `_ViewStart.cshtml` (containing proper URL to _Layout.cshtml file) files in the Views folder of the area for proper UI layout and for the tag helpers to work.

    **Index**: lists the selected user's devices in a table element. The table has header row and columns for Id, UserId, Name, Description and DateAdded values and a column for Details, Edit and Delete action links to the device. The view also has a link to create a new device for the selected user (i.e. a link to `Create` action).

    **Details**: shows the selected user's selected device's data with links to Edit and Delete actions for the device.

    **Create**: renders a html form to create a new device. Only `Name` and `Description` property values can be set by the manager user on the form. Other values are set by the controller action (i.e. by the server).

    **Edit**: renders a html form to edit the selected device. The form must have a input field for all of the device's properties but only `Name` and `Description` should be editable by the manager. Use html `readonly` attribute to other inputs to indicate that the user should not edit those.

    **Delete**: shows the selected user's selected device's data. Renders a html form and submit button to confirm the deletion. Clicking the submit button posts (HTML POST) the form and the server handles the device deletion. The html form must contain hidden input fields for the device id (Id) and the user id (UserId). The delete action uses the values from the POST message's body to delete the selected user's selected device.

3. Normal user's functionality

    Implement `DevicesController` class that has all the functionality for a normal user. The class is in namespace `DeviceManager.Controllers`. Implement the class so that anonymous users cannot access any of the action methods. Use properly configured Authorize attribute with the DevicesController class. Url https://localhost:<port>/devices must be handled by Index action method in the DeviceController class. Note that normal user's functionality is implemented without areas (i.e. as default MVC style). Use `DeviceViewModel` or `collection of DeviceViewModels` as the model class to the views.

    **Index**: lists the logged-in user's devices in a table element. The table has header row and columns for Id, UserId, Name, Description and DateAdded values and a column for Details, Edit and Delete action links to the device. The view also has a link to create a new device for the logged-in user (i.e. a link to `Create` action).

    **Details**: shows the logged-in user's selected device's data with links to Edit and Delete actions for the device. Must be accessible from url ../devices/details/[id] where [id] is the selected device's id. Must only show the details if the device with id is actually the logged-in user's device.

    **Create**: renders a html form to create a new device. Only `Name` and `Description` property values can be set by the user on the form. Other values are set by the controller action (i.e. by the server). Note! the device's owner is also set by the action method in the server.

    **Edit**: renders a html form to edit the selected device. The form must have a input field for all of the device's properties but only `Name` and `Description` should be editable by the user. Use html `readonly` attribute to other inputs to indicate that the user should not edit those.

    **Delete**: shows the logged-in user's selected device's data. Renders a html form and submit button to confirm the deletion. Clicking the submit button posts (HTML POST) the form and the server handles the device deletion. After successful delete operation the user is redirected to DevicesController's Index action method.

4. Administration area

    All the content in the Administration area is accessible only to users who are in `Admins` role. The area's main page (/administration) must be handled in the area's HomeController's Index action method. The rendered view contains a table element with all the users (`DeviceManagerAppUser` class instances) in the application's database. The table has header row and columns for a `user's id` and `email` fields, a column for `user's roles` and a column for link element (`a` element) to the user details. Controllers for Administration area are in namespace `DeviceManager.Areas.Administration.Controllers`.

    User details is implemented in `UserDetails` action of area's `ManageController`. The user details action shows the user details (user id, email, user roles) and links to delete user and manage roles actions. Must be accessible from ../administration/manage/userdetails/[id] where [id] is the selected user's id.

    Delete user action is implemented in `DeleteUser` action of area's `ManageController`. Use normal MVC-style GET and POST action methods for the delete operation. Must be accessible from ../administration/manage/deleteuser/[id] where [id] is the selected user's id. After successful deletion the user is redirected to the area's main page (/administration). The delete operation must succeed even when the user has devices and/or roles. The possible user's roles and devices are also deleted.

    Manage roles action is implemented in `UserRoles` action of area's `ManageController`. If the user belongs to role `Managers` then submitting the form removes the user from the role. And if the user does not belong to role `Managers` then submitting the form adds the user to the role (i.e. the form works as a toggle for the Managers role). Handle the modifications in POST version of the UserRoles action (i.e. the same way that you would implement default Create, Edit or Delete actions in an MVC app). Must be accessible from ../administration/manage/userroles/[id] where [id] is the selected user's id. Notice that the controller action needs to handle only the Managers role. After successful role modification the user is redirected to the user details page (/administration/manage/userdetails/[id]).

    > Note that you may need `_ViewImports.cshtml` and `_ViewStart.cshtml` (containing proper URL to _Layout.cshtml file) files in the Views folder of the area for proper UI layout and for the tag helpers to work.    

5. Resource based authorization cases
    
    **Case 1**: 
    A device with id D belongs to a user with id X. A normal user with id Y logs in to the application and types the url https://localhost:<port>/devices/details/D (i.e. tries to access to a device belonging to another user). This must not be allowed. 

    **Case 2**:
    A device with id D belongs to a user with id X. A normal user with id Z logs in to the application and edits the form content to make a HTTP POST request to the url https://localhost:<port>/devices/delete/D (i.e. tries to POST a delete request to a device belonging to another user). This must not be allowed.

    The app must not cause 500 errors in any of these cases (InternalServerError).

    In all cases where a user tries to CRUD (Create, Read, Update or Delete) another user's device. The action must not be allowed to complete (i.e. the data is not shown to the wrong user and changes are not persisted to the database). The fraudulent user must instead be redirected to the Error action on the HomeController. 

    > If the user is in Managers role (i.e. a manager), the above still apply. A manager can CRUD other user's devices only through the Management area. If the manager is using the Devices controller's actions, then he acts as a normal user.

> **Note!** Read the task description and the evaluation points to get the task's specification (what is required to make the app complete).

## Task evaluation

Evaluation points for the task are described above. An evaluation point either works or does not work there is no "it kind of works" step in between. Be sure to test your work. All working evaluation points are added to the task total and will count toward the course total. The task is worth twenty (20) points. Each evaluation point is checked individually and each will provide four (4) points so there are five checkpoints. Checkpoints are designed so that they may require additional code, that is not checked or tested, to function correctly.

## DevOps

There is a DevOps pipeline added to this task. The pipeline will build the solution and run automated tests on it. The pipeline triggers when a commit is pushed to GitHub on the main branch. So remember to `git commit` and `git push` when you are ready with the task. The automation uses GitHub Actions and some task runners. The automation is in the folder named .github.

> **DO NOT modify the contents of .github or tests folders.**
