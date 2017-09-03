:no_entry: [DEPRECATED] This application has been deprecated in favor of [Imposter for Fiddler](https://github.com/gotdibbs/Imposter.Fiddler) which is an extension for Fiddler I wrote based off of this codebase. There are more features and it is a lot more stable as an extension. This repo will be left active for historical purposes.

Imposter
========

Imposter looks for requests matching a base URL and then swaps them for a matching local file. You can think of it as a version of Fiddler's AutoResponder that handles whole directories instead of single files.

>
<img alt="ScreenShot" src="https://raw.github.com/gotdibbs/Imposter/master/Screenshot-main.png" style="border: 1px solid #444;" />

### Installation

1. [Install the ClickOnce app from here](https://raw.github.com/gotdibbs/Imposter/master/ClickOnce/setup.exe) (or download, build and publish locally)

### Configuration

1. **Profile Name:** Set it to whatever you would like. I usually set it to the project that I am working on.
2. **Base URL:** The URL that will be used as the base fragment to match your local files off of. It can be a full valid URL, or a fragment such as `/js/`. Just note that whatever comes after your Base URL will be used to match files. If your Base URL is `/js/` and a URL is requested matching `/js/min/test.min.js`, we will look in the local directory for a folder named `min` with a file called `test.min.js` in it.
3. **Local Directy:** The directory that is searched for matching files to be served in place of remote files.
4. **Port:** The port the proxy is to run on.
5. **Decrypt HTTPS Traffic:** Will install a certificate created by Fiddler to decrypt traffic over HTTPS.
6. **Overrides:** A listing of specific hard coded fallbacks for when a requested resource is unable to be located. Very helpful for hot swapping minified for unminified files for debugging.

>
<img alt="ScreenShot" src="https://raw.github.com/gotdibbs/Imposter/master/Screenshot-profile.png" style="border: 1px solid #444;" />
