dbdump
======

A little command line tool that will dump a SQL Server database in a format that can be imported by MySQL.

##What you'll need

In order to run dbdump you'll need the following:

1. A computer running SQL Server. You can download SQL Server 2008 Express [here](http://www.microsoft.com/en-ca/download/details.aspx?id=1695) and SQL Server 2008 Management Studio Express [here](http://www.microsoft.com/en-ca/download/details.aspx?id=7593).
2. A computer with the .net framework 4.x. You can download it [here](http://www.microsoft.com/en-ca/download/details.aspx?id=40779).

##Restoring a SQL Server Backup

1. SQL Server backups usually have a .bak extension. If your backup file doesn't have this extension then there's a good chance that you have the wrong file.
2. Launch SQL Server Management Studio.
3. Connect to the appropriate server.
4. Right click on the **Databases** folder on the left.
5. Choose **Restore Backup…**
6. Choose **From Device**.
7. Browse to the location of your .bak file and select it.
8. You should now see a list of backups. Most of the time there is just one. Select the appropriate backup.
9. Select the database to restore to. You can type the name of a database that doesn't exist to create a new database.
10. Click OK and the database should restore.

##Running dbdump.exe

1. dbdump.exe has an associated config file. Make sure you edit this file and enter your SQL Server information so that it knows which server to connect to.
2. Once you've set up your config file you can type **dudump** to see a list of command line options.

##Connection Strings

Your config file requires a connection string in order to connect to your SQL server. If you program against a SQL server then this should be familar to you. If you aren't too sure you can go to [connectionstrings.com](https://www.connectionstrings.com/sql-server/) to read up on their documentation.

###Connecting with Windows Authentication

If you are running dbdump on the same computer as your SQL server then you will most likely be able to use windows authentication. Your connection string will look something like this:

```
Data Source=<Server Name>;Application Name=dbdump;Integrated Security=true;
```

###Connection with a Username/Password

If you are running dbdump from a different computer such as your web server then you might have to use a connection string with a username/password. It will look something like this:

```
Data Source=<Server Name>;Application Name=dbdump;Initial Catalog=<Database Name>; User ID=<Username>;Password=<Password>
```
