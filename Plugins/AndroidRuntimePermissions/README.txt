= Android Runtime Permissions (v1.2.0) =

Online documentation & example code available at: https://github.com/yasirkula/UnityAndroidRuntimePermissions
E-mail: yasirkula@gmail.com

1. ABOUT
This plugin helps you query/request runtime permissions on Android M and later.

2. HOW TO
You can use the following static functions of AndroidRuntimePermissions to manage runtime permissions:

- bool CheckPermission( string permission ): checks whether or not the permission is granted

- bool[] CheckPermissions( params string[] permissions ): queries multiple permissions simultaneously. The returned array will contain one entry per queried permission

- Permission RequestPermission( string permission ): requests a permission from the user and returns the result. It is recommended to show a brief explanation before asking the permission so that user understands why the permission is needed and doesn't click Deny or worse, "Don't ask again". Permission is an enum that can take 3 values:
-- Granted: permission is granted
-- ShouldAsk: permission is denied but we can ask the user for permission once again. As long as the user doesn't select "Don't ask again" while denying the permission, ShouldAsk is returned
-- Denied: we don't have permission and we can't ask the user for permission. In this case, user has to give the permission from app's Settings. This happens when user selects "Don't ask again" while denying the permission or when user is not allowed to give that permission (parental controls etc.)

- Permission[] RequestPermissions( params string[] permissions ): requests multiple permissions simultaneously

- Task<Permission> RequestPermissionAsync( string permission ): asynchronous version of RequestPermission. Unlike RequestPermission, this function doesn't freeze the app unnecessarily before the permission dialog is displayed

- Task<Permission[]> RequestPermissionsAsync( string[] permissions ): asynchronous version of RequestPermissions

- void OpenSettings(): opens the settings for this app, from where the user can manually grant permission(s) in case a needed permission's state is Permission.Denied