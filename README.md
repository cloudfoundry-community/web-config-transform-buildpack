### Web Config Transform Buildpack

A supply buildpack that will transform web.config based on the standard XSD transform templates. ASPNETCORE_ENVIRONMENT environmental variable is used to select the profile to be used. Ex. `ASPNETCORE_ENVIRONMENT=Debug`, results in `Web.Debug.config` being applied on top of `Web.Config`.

Any environment variable set with matching key configured in web.config `/configuration/appSettings` will have their values replaced.

If config server binding is detected, it will replace any matching token in web.config with config server value. Tokens are specified in format of `#{config:path}`

### Usage

Include buildpack in applications `manifest.yaml`. Example:

```yaml
applications:
- name: simpleapp
  stack: windows2016
  buildpacks: 
    - https://github.com/macsux/web-config-transform-buildpack/releases/download/1.0/web-config-transform-buildpack.zip
    - hwc_buildpack
```

