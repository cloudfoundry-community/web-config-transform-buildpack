## Web Config Transform Buildpack

### Purpose of the `web-config-transform-buildpack`

Cloud Native Applications are expected to bring in configurations from external sources like environment variables, config server , etc. Please refer to `Configuration` in [12factor.net](https://12factor.net) for more information.

In legacy ASP.Net applications, configuration settings are injected through `web.config` files, and in Console applications, configuration settings are injected through `app.config`. As per cloud native principles, configuration should stay out of build artifacts. In this recipe we will use a custom buildpack which provides a solution to this problem by using token replacement during cf push staging.


### High level steps

1. Identify environment dependent configurations and externalize
1. Create app manifest
1. Add [web|app] config transformations.
1. Move config settings to Spring Cloud Config Server
1. Create service for Spring Cloud Config Server 
1. Bind config service to app using manifest
1. Push app by parameterized environment name

#### 1. How the buildpack works

* Pulls all the configurations from environment variables and config server repo (if bounded).
* Config server environment is identified by the environment variable `ASPNETCORE_ENVIRONMENT`, e.g. `dev`, `prod`, etc.
* Apply xml transformation
	> The transformation file target is pulled from environment variable `XML_TRANSFORM_KEY`. 
    The pattern, `[web|app].{XML_TRANSFORM_KEY}.config`, is used to identify the file to transform. 
    For e.g. if the value of environment variable `XML_TRANSFORM_KEY` is `CF`, then the transformation file, the buildpack looks for, is `web.CF.config` for web applications and `app.CF.config` for console applications. 
    If the `XML_TRANSFORM_KEY` is not set, it looks for `web.Release.config` or `app.Release.config` by default. 
    If the file doesn't exist, it skips transformation step and moves further. 
    Note that the transformation filenames are case sensitive and should match the case used in `XML_TRANSFORM_KEY`.
    Lastly, the final transformed config file name for console applications is `{applicationName}.exe.config` instead of `web.config` for web applications.
* Modify the transformed file with `appSettings:key` for `<AppSettings>` section and `connectionStrings:name` for `<ConnectionStrings>` section 
* Modify the transformed file with tokens provided in the format `#{anykey}`, e.g. A token named `#{foo:bar}` will replaced with `myfoovalue` if an environment variable with key `foo:bar` is set with value `myfoovalue` or the config server repo `yaml` contains the info as below.

```yml
foo:
  bar: myfoovalue
```

  > NOTE: all transform xml attributes and tokens are case-sensitive

##### Execution steps in detail

* web.config (Before transformation)

```xml
<connectionStrings>
    <add name="MyDB" 
         connectionString="Data Source=LocalSQLServer;Initial Catalog=MyReleaseDB;User ID=xxxx;Password=xxxx" />
</connectionStrings>
```

* web.CF.config (Transformation file)

```xml
<connectionStrings>
    <add name="MyDB" 
         connectionString="#{connectionStrings:MyDB}" 
         xdt:Transform="SetAttributes" 
         xdt:Locator="Match(name)"/>
</connectionStrings>
```

* If `XML_TRANSFORM_KEY` is set to `CF`

* web.config (after transformation)

```xml
<connectionStrings>
    <add name="MyDB" 
         connectionString="#{connectionStrings:MyDB}"/>
</connectionStrings>
``` 

* If `ASPNETCORE_ENVIRONMENT` is set to `dev` and the config server repo `yaml` is as below...

```yml
connectionStrings:
  MyDB: "Data Source=11.11.11.11;Initial Catalog=mydb;User ID=xxxx;Password=xxxx"
```

* web.config (after token replacement)

```xml
<connectionStrings>
    <add name="MyDB" 
         connectionString="Data Source=11.11.11.11;Initial Catalog=mydb;User ID=xxxx;Password=xxxx"/>
</connectionStrings>
``` 

> Note: For a console applications, the transformation file, in the above example, would have been called app.CF.config and the resulting transformed file would be the application exe name followed by the `.exe.config` extension.

#### 2. Create a Cloud Foundry app manifest  

* Ensure your application has a Cloud Foundry manifest file. If your application is in Cloud Foundry already, you can create the manifest using the command `cf create-app-manifest [appname]`. 
* Add a buildpack reference to the manifest (before the hwc buildpack) that will perform the token replacement on cf push action.  
    >Note: Please refer to https://github.com/cloudfoundry-community/web-config-transform-buildpack/releases to pull the latest version as appropriate. `XXX` refers to the version of buildpack.  
* Add an environment variable to the manifest for each config item that will be used to replace the tokenized values. Below is a sample added referring to the connection string. 
    > Note: Adding token replacements with Environment variables is only for experimental activities. Config settings should be externalized using git repositories and Spring Cloud Config Server.


```yaml
applications:
- name: sampleapp
  stack: windows
  buildpacks:
  - https://github.com/cloudfoundry-community/web-config-transform-buildpack/releases/download/vXXX/Web.Config.Transform.Buildpack-win-x64-XXX.zip
  - hwc_buildpack
  env:
    "connectionStrings:MyDB": "Data Source=ReleaseSQLServer;Initial Catalog=MyReleaseDB;User ID=xxxx;Password=xxxx"
```

##### Note  
> Only put configuration item keys and values in the manifest for testing purposes. Spring Cloud Config Server should be used for externalizing configuration settings (see further sections).


#### 3. Add web config transformations

By default, all web apps and wcf apps created with **Debug** and **Release** configurations and corresponding web config transformation files (web.Debug.config, web.Release.config).

* Add required transformations to `web.Release.config`

* Build and push the application to Cloud Foundry to verify that your config settings are properly transformed

###### Note
> For developer machine debugging, use `Debug` configuration profile and `web.Debug.config` for transformation.


Sample `web.Release.config` with transformations

```xml
<?xml version="1.0" encoding="utf-8"?>
<!-- For Cloud Foundry -->
<configuration xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
  
  <connectionStrings  xdt:Transform="Replace">
    <add name="MyDB" connectionString="#{connectionStrings:MyDB}" providerName="System.Data.SqlClient"/>
  </connectionStrings>
  
  <system.serviceModel>
    <client xdt:Transform="Replace">
      
      <endpoint 
        address="#{client:Default_IMyLogService:address}" 
        binding="#{client:Default_IMyLogService:binding}" 
        bindingConfiguration="#{client:Default_IMyLogService:bindingConfiguration}"
        contract="ServiceProxy.IMyLogService" name="Default_IMyLogService" />
    
    </client>
  </system.serviceModel>

</configuration>
```

#### 4. Move config settings to Spring Cloud Config Server 

A multi-environment, production-ready configuration can be achieved using share and environment specific transforms and using Spring Cloud Config Server - backed by a git repository data source.

1. Create a network accessible git repository for each application
1. Create <YOUR_APPLICATION>.yaml file to have common settings across all environments
1. Create <YOUR_APPLICATION>-< YOUR_APP_ENVIRONMENT>.yaml for each unique environment
1. Specify your environment  with `ASPNETCORE_ENVIRONMENT` environment variable in the manifest file created in step 2. e.g: `ASPNETCORE_ENVIRONMENT=Production`

##### Sample Config Server yml files

sampleapp.yaml
```yaml
appSettings:
  Setting1: "Common setting1"
```

sampleapp-Development.yaml
```yaml
 connectionStrings:
   MyDB: "Data Source=devserver;Initial Catalog=mydb;User ID=xxxx;Password=xxxx"
```

sampleapp-Production.yaml
```yaml
 connectionStrings:
   MyDB: "Data Source=prodserver;Initial Catalog=mydb;User ID=xxxx;Password=xxxx"
```

#### 5. Create service for Spring Cloud Config Server

1. Make sure you have config server available in your CF marketplace. 
    > NOTE: To check if you have this server, run `cf marketplace`. You should see `p.config-server` or `p-config-server` in this list. 

1. Create a JSON file for config server setup (ex: config-server.json)
    ```json
    {
        "git" : { 
            "uri": "https://github.com/YOUR_USERNAME/YOUR_CONFIG_REPO"
        }

    }
    ```
    > NOTE: Ensure file is not BOM-encoded
    
1. Create config server using above configuration file.
    ```script
    cf create-service p-config-server standard my_configserver  -c .\config-server.json
    ```

#### 6. Bind config service to app using manifest

Bind the config server to your app once the config server service is created. Add your config server to the `services` section as seen below:

```yaml
---
applications:
- name: sampleapp
  stack: windows
  buildpacks: 
    - https://github.com/cloudfoundry-community/web-config-transform-buildpack/releases/download/vXXX/Web.Config.Transform.Buildpack-win-x64-1.1.5.zip
    - hwc_buildpack
  env:
    ASPNETCORE_ENVIRONMENT: ((env))

  services:
  - my_configserver
```

#### 7. Push app by parameterized environment name

Parameterizing your application environment gives ability to provide different value as per you deploy stage in CD pipelines. e.g: Development/QA/UAT/Production.

This can be achieved by replacing hardcoded value of `ASPNETCORE_ENVIRONMENT=YOUR_DEPLOY_ENVIRONMENT` with `ASPNETCORE_ENVIRONMENT: ((env))`

Now you can push your app with below command

```script
cf push --var env=Production
```

You should be able to find if the environment value is actually passed in, by looking at logs.

```
================================================================================
=============== WebConfig Transform Buildpack execution started ================
================================================================================
-----> Using Environment: Production
-----> Config server binding found...
```


#### Special Behavior for appSettings and connectionStrings
This buildpack makes it possible to externalize appSettings and connectionString values in your web.config without using tokenized values. In this case simply include the values in your yaml config files on your Github repository (`{YOUR-APP}.production.yml`, `YOUR-APP}.development.yml`, etc.

sampleapp-Development.yaml
```yaml
appSettings:
  Setting1: "development setting"
 connectionStrings:
   MyDB: "Data Source=devserver;Initial Catalog=mydb;User ID=xxxx;Password=xxxx"
```

sampleapp-Production.yaml
```yaml
appSettings:
  Setting1: "production setting"
 connectionStrings:
   MyDB: "Data Source=prodserver;Initial Catalog=mydb;User ID=xxxx;Password=xxxx"
```

This buildpack can inject appSettings and connectionStrings values based on environment specific yaml config files even if replacement tokens are not present in web.Release.config file.

### Sample Application & Walkthrough
A sample web application and walkthrough can be found [here](https://github.com/cloudfoundry-community/webconfig-example-app)

#### Note
> For any issues you face with the web-config-transform-buildpack, please raise an issue at https://github.com/cloudfoundry-community/web-config-transform-buildpack/issues.


