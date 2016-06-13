NTIS Subscriber Service example - README
========================================

This project is an example implementation of the NTIS Subscriber Service.
It has been created in, and tested with, Visual Studio 2012 and the .NET framework version 4

For the instructions below, it is assumed that

- the user is familiar with Visual Studio
- the user is familiar with IIS (If not, IIS can be run by typing IIS from the windows command prompt)
- the user is familiar with the SOAPUI testing and SOAPUI has been installed on the development machine. See http://www.soapui.org/Downloads/latest-release.html

Building the web service
------------------------

- Open a Windows explorer and navigate to SubscriberWebService.sln solution
- In the solution explorer, select the solution, then select 'Build Solution' from the Build Menu
- The web service is configured to received 512MB compressed data. To change this, change the web.config file

Note:

- There is an auto generated file, supplierPush.cs, built using the customer provided wsdl and xsd files. These files are held in sub folders in the folder subscriberservice.net\SubscriberService\SubscriberWebService\Contract
- The command used to build supplierPush.cs can be found by selecting the SubscriberWebService properties from the solution explorer, then navigating to the Build Events Tab
- The implementation contains the wsdl and xsd used to generate the WCF service proxy, if regenerating the proxy from these files svcutil will set ReplyAction = "*" which causes the operation not to appear on the wsdl generated dynamically by IIS. This will cause new applications referencing the service to not find the push operation, it should not however affect existing applications

Initial testing of the web service
----------------------------------

For initial testing, you may wish to use the development server that visual studio is equipped with:

- Build the solution using the instructions in 'Building the web service'
- Comment out the command used to build the supplierPush.cs file (See above)
- Edit the supplierPush.cs file and in the public interface 'supplierPushInterface', change the ReplyAction from "*" to "http://datex2.eu/wsdl/supplierPush/2_0/putDatex2Data"
- Build the solution using the instructions in 'Building the web service'
- Open the 'DEBUG' dropdown menu, and click 'Start Debugging'

Note: Visual Studio may fail to launch the Development Server if IIS is installed and running with the same port number configured. To correct this, either

- In IIS, stop the site which has the same port number
- In Visual Studio, open the SubscriberWebService properties from the solution explorer, navigate to the Web tab and change the port number

Deploying the web service
-------------------------

To deploy the web service

- Select the SubscriberWebService from the solution explorer and select Publish Selection from the Build Menu
- Select Publish when the Publish Web window appears

The above instructions will publish to the C:\inetpub\wwwroot\Datex2 folder

- Open IIS from the windows command prompt
- Navigate to Sites/Default Web Site
- Select Datex2, right click and select "Convert To Application", using the defaults provided when prompted

Note: If deploying locally, you may need to run visual studio as administrator to publish, or else change security permissions for the webroot.

You should now be able to navigate to the location in your browser (e.g 'http://localhost/Datex2/WebServiceSubscriber.svc').

Note: If this fails, check:

- The Default Web Site is running
- The port number of the Default Web Site (found under the bindings option) is set to 80. If not, change the URL to http://localhost:<Port Number>/Datex2/WebServiceSubscriber.svc or reset the port number to 80, restart the Default Web Site and refresh the web page

Note: You may need to register .NET 4 with IIS, this can be done using the ASP.NET IIS Registration Tool ('Aspnet_regiis.exe'). To register .NET 4 with IIS, run "Aspnet_regiis.exe -i" from the Visual Studio Developer Console.
	
Note: If you receive a server error related to 'targetFramework' check that the correct .NET version is set in your application pool. Application pools can be viewed in the IIS Manager, which can be found in control panel, under administrative tools.

This example implementation should be compatible with any .NET 4 version (e.g 4.0 or 4.5).

Testing the website 
-------------------

All examples are held in the exampleRequests folder

There is also a SOAPUI project which holds these examples and this is held in the SoapUI Tests folder. Both sets of examples conform to the 2.5 standard

### Testing Using SoapUI

SoapUI is an open source cross-platform tool which can be used for testing SOAP requests and responses.
The version used for testing this example was V4.5.2 Because of its simple interface, it was used as a client for testing this Subscriber web service.
When testing using SOAPUI, ensure the compression option in Preferences is set to the compression type in the web.config file

To send a request:
- Make sure that the Subscriber web service is running (either through IIS or the Development Server)
- Start soapUI.
- From the main menu select File -> New soapUI Project.
- Enter a Project Name, browse to the WSDL or manually enter the location. (e.g. 'http://localhost/Datex2/WebServiceSubscriber.svc')
- Click "OK".
- The service and its operations will then be exposed.
- Modify any of the requests and enter suitable values, or copy any of the example messages provided in the "exampleRequests" folder, which match the message to be tested, and paste over the contents of the "Request 1" sample generated.
- Click the green play arrow at the top of the request and check that a success response is sent.
The message requests and responses will be logged in 'C:\temp\'.
	
To use the included SoapUI tests.
- Import the file '\SoapUI Tests\SubscriberService-soapui-project.xml' into SoapUI. The URL will need to be changed if running remotely
You should now see a series of tests which can be executed in the left hand navigation pane.

