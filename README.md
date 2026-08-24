# Borocito-CMD
Control panel for Borocito  

This is the official and recommended way to control Borocito instances.  

## Features

 - Control multiple instances.  
 - Check telemetry and files uploaded by instances.  
 - Easy as fuck to use.  
 - Windows native UI.  
 - Configurable.  
 - Official Borocito Software.  

## Usage

First, you need to have the [Borocito Server-Side](https://github.com/Borocito/Borocito-Server) already setup and configured.  

When start by the first time, the CMD will ask you some information:  

 1. OwnerServer: The FULL URL where Borocito Server-Side is. Example https://example.com/Borocito.  
 2. Host Address: The FTP HOST URL for connect via FTP. Example: ftp://example.com  
 3. Host User: The User FTP Account to connect to the Host Address FTP server.  
 4. Host Password: The Password for the FTP User Account.  

And that's it. You should have CMD configured correctly. You can now see instances, telemetry and users.  

If not, you have failed to configure CMD. You can modify the values by going to the Windows Registry:  
```
HKEY_CURRENT_USER\SOFTWARE\Borocito\Control
```

## TODO
Ha pasado un largo tiempo, y me sorprende lo que he creado. Pero ahora estoy viejo, y no entiendo como logre tal nivel de perfeccion.  
Lo que quiero decir es que, demorare un tiempo en adaptar el funcionamiento para Borocito 0.3.0.0  
