package com.yasirkula.unity;

import android.annotation.TargetApi;
import android.app.Activity;
import android.app.Fragment;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.Settings;

import java.util.ArrayList;

/**
 * Created by yasirkula on 27.04.2018.
 * Based on UnityAndroidPermissions (MIT License): https://github.com/Over17/UnityAndroidPermissions
 */

public class RuntimePermissions
{
	// Credit: https://stackoverflow.com/a/35456817/2373034
	public static void OpenSettings( final Context context )
	{
		Uri uri = Uri.fromParts( "package", context.getPackageName(), null );

		Intent intent = new Intent();
		intent.setAction( Settings.ACTION_APPLICATION_DETAILS_SETTINGS );
		intent.setData( uri );

		context.startActivity( intent );
	}

	@TargetApi( Build.VERSION_CODES.M )
	public static String CheckPermission( final String[] permissions, final Context context )
	{
		String result = "";
		if( Build.VERSION.SDK_INT < Build.VERSION_CODES.M )
		{
			ArrayList<Integer> permissionsResult = new ArrayList<Integer>( permissions.length );
			for( int i = 0; i < permissions.length; i++ )
				permissionsResult.add( 0 );

			try
			{
				String[] requestedPermissions = context.getPackageManager().getPackageInfo( context.getPackageName(), PackageManager.GET_PERMISSIONS ).requestedPermissions;
				if( requestedPermissions != null )
				{
					int remainingPermissions = permissions.length;
					for( String requestedPermission : requestedPermissions )
					{
						int i = 0;
						while( i < permissions.length && !permissions[i].equals( requestedPermission ) )
							i++;

						if( i < permissions.length )
						{
							permissionsResult.set( i, 1 );

							if( --remainingPermissions <= 0 )
								break;
						}
					}
				}
			}
			catch( PackageManager.NameNotFoundException e )
			{
			}

			for( int i = 0; i < permissionsResult.size(); i++ )
				result += permissionsResult.get( i );
		}
		else
		{
			for( int i = 0; i < permissions.length; i++ )
				result += context.checkSelfPermission( permissions[i] ) == PackageManager.PERMISSION_GRANTED ? '1' : '0';
		}

		return result;
	}

	public static void RequestPermission( final String[] permissions, final Context context, final RuntimePermissionsReceiver permissionReceiver, final String lastCheckResult )
	{
		String permissionResult = CheckPermission( permissions, context );
		if( Build.VERSION.SDK_INT < Build.VERSION_CODES.M )
		{
			permissionReceiver.OnPermissionResult( permissionResult );
			return;
		}

		boolean shouldShowPermissionDialog = false;
		for( int i = 0; i < permissions.length; i++ )
		{
			if( permissionResult.charAt( i ) == '0' && lastCheckResult.charAt( i ) != '0' )
			{
				shouldShowPermissionDialog = true;
				break;
			}
		}

		if( !shouldShowPermissionDialog )
			permissionReceiver.OnPermissionResult( permissionResult );
		else
		{
			Bundle bundle = new Bundle();
			bundle.putStringArray( RuntimePermissionsFragment.PERMISSIONS, permissions );

			final Fragment request = new RuntimePermissionsFragment( permissionReceiver );
			request.setArguments( bundle );

			( (Activity) context ).getFragmentManager().beginTransaction().add( 0, request ).commitAllowingStateLoss();
		}
	}
}
