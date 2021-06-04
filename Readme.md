This project is used to access Xerox Docushare.

Most of the interactions offered through the controllers classes is readonly, directly from the Docushare Database. See web.config connection string.
However, there are write operations issed through the wellregcontroller. This controller uploads well registry documents, and it uses the services below

Services used:
	-dsTools (eforms manager)
	-arTools (eforms manager)
	-Armanager (eAR AR_Webservice)

Database tables used:
	-DSObjects_table
	  -This is the main table used on the backend of the Docushare system that houses Docushare Object information including custom objects and their metadata fields


Install
Check the publish profile - right-click on the project in the Solution Explorer.

Controllers
The Controllers are split up so that each group in the department has their own controller.
There is some duplication of code and so a pattern, such as a repository, should be used to DRY it up.

Util helper class
This class houses the functions to call the Xerox Docushare Developer API (login, property find, upload, lock/unlock)
NOTE: the git branch named "ConsoleProj" has an added .net console project where instead of using the httpWebRequest .net api
  The newer HttpCLient api was used successfully. This is the prefered HTTP api as it is still in support and has asynchronous capabilities
NOTE: This branch has an updated .net framework version and hasn't been fully tested


Models
armFolder - models arm queries
DSFileInfo - models basic/shared information that most Docushare Objects use
DSObject.edmx - Appdev generally hasn't used this method of entity framework entity modeling, but this allows visual/designer modeling of database objects
Scan - model used for the adwr_admin.adwr_scan table (currently only used for wells group, but is general enough for other use cases)
WellRegDoc - models pertinent data for well registry documents

This project uses an older version of .net and entity framework.
Because of this, some of the features that are used in other web apis (hydrosapi) may not be available or work differently
To upgrade the project, it's probably best to start a new  web api project using the latest .net framework and copy the controllers/models over one at a time.

