![Flush][logo]

## What is this?

Flush is a tool for facilitating and enhancing Agile planning activities; built with speed, ease of use and reliability in mind.

## Prerequisites

- Docker 17.06+.

A previous version of this document indicated the .NET Core 5.0 SDK as a prerequisite. The .NET Core 5.0 SDK is provided by an intermediate container during the build process, and is no longer required for the application as-shipped. However, should you wish to build and run Flush without the containers, you should ensure you have the .NET Core 5.0 SDK installed.

## Configuring the Application

Flush uses X509 certificates in key derivation processes during the application runtime. You may use the same certificate for all purposes, though this is not recommended. The steps below illustrate how to configure one certificate, though these steps should be run once for each of the FlushDb, IdentityDb and JwtAuthentication sections of the appsettings file.

```
# generate a private and public key
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout cert.key -out cert.crt

# export the thumbprint - note this down
openssl x509 -in cert.crt -noout -fingerprint

# export it as a PKCS12 file
openssl pkcs12 -export -out cert.pfx -inkey cert.key -in cert.crt

# clean up
rm cert.key cert.crt
```

You should then, for each certificate, add the thumbprint into the appsettings file as follows.

```
"YourCertSection": {
   "HashAlgorithm": "SHA512",
   "Thumbprint": "<the thumbprint you exported goes here.>"
}
```

In this example, "YourCertSection" is a placeholder for the appropriate section of the config file: JwtAuthentication, FlushDb or IdentityDb.

We include a script, `./gen_certs.sh`, which automates all of the above steps.

## Running the Application

Before you may run Flush, you will need to build the application and containers. Start by checking out the code, then use the `./build.sh` script to automatically compile and build the application and its container image. Lastly, run `./start.sh` to begin execution of the flush container.

```
# clone the repo
git clone git@github.com:ChampionOfGoats/Flush.git

# change into the app root directory
cd Flush

# initialise the submodules
git submodule init
git submodule update

# generate a set of certificates
./gen_certs.sh --clean

# build the container image
./build.sh

# start the container
./start.sh --cert <your ssl cert> \
    --cert-password <your ssl cert password> \ # only include this argument if your ssl cert is password protected.
    --cert-dir <the location of your ssl cert> \
    --data-dir <the location to store sqlite databases>
```

Flush may be run behind a mature reverse proxy such as IIS or NGINX. Please refer to the documentation of your preferred reverse proxy software for guidance on configuring this.

## Changelog

See [this document](Docs/CHANGELOG.md).

## Known Issues

See [this document](Docs/KNOWNISSUES.md).

[logo]: Docs/flush-logo.png
