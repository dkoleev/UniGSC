Google Sheets Configs for Unity game engine.
===

## Table of contents

<!-- toc -->
- [Installation](#installation)
- [Setup](#setup)
- [Creating Credentials in Google API Console](#creating-credentials-in-google-api-console)
- [Connect Unity project to Google Sheets service](#connect-unity-project-to-google-sheets-service)
- [Setup local configs](#setup-local-configs)
  * [Example](#example)
<!-- tocstop -->

## Installation
Use [UPM](https://docs.unity3d.com/Manual/upm-ui.html) to install the package via the following git URL: 

```
https://github.com/dkoleev/GoogleSheetsConfig.git
```

![](https://gyazo.com/8c8fc97345fc64f53d62814cce571974.gif)


## Setup
### Creating Credentials in Google API Console
To set up **OAuth** or **API Key** authorization, follow these steps:
- Open [Google API Console](https://console.developers.google.com/) and select the **Credentials** section under APIs & Services.

  ![image](https://user-images.githubusercontent.com/54948242/212972962-fabc6862-6540-40f7-b1d0-3359c662ebf2.png)
  
- Select Google APIs and Services project for your Unity application. If you do not have project select **CREATE PROJECT** to create new project. 
- Enable the Google Sheets API support.
- Select the **CREATE CREDENTIALS** option and select either **API Key** or **OAuth Client ID**.
#### OAuth
Wigh OAuth 2.0 authentication you can read and write from a public and private sheets. For more information, see [Google’s OAuth 2.0 documentation](https://developers.google.com/identity/protocols/oauth2).

When generating OAuth credentials, set the application type to Desktop (because plugin uses the service only through the Unity Editor).

After the credentials have been created, download the JSON file.

![image](https://user-images.githubusercontent.com/54948242/212972140-70c60a83-b3fa-4c71-bb9d-137564c71c4b.png)
![image](https://user-images.githubusercontent.com/54948242/212972417-42ed6fc2-e799-47a3-b9d6-701e14e542c1.png)

#### API Key
API keys are a simple encrypted string that can be used only for read data from public Google Sheets.

After the key has been created, click **SHOW KEY** and copy key to clipboard.

### Connect Unity project to Google Sheets service
- Create **Google Sheets service Provider**. 
  - Right Click in **Project** tab.
  - Select `Create -> Yogi -> Google Sheets Configs -> Provider`
  - Select created file.
  - Choose authentication type
  
    ![image](https://user-images.githubusercontent.com/54948242/212975382-19a3df8a-e81a-47ec-9437-ddf8fae8a7d3.png)
    
  - OAuth
    - Click `Load Credentials` and select credentials .json file downloaded from Google Sheets API.
    - Click `Authorize`.
   - Api Key
      - Insert **Api Key** to field.
  ### Setup local configs.
    - Create **Google Sheets Configs** file. 
      - Right Click in **Project** tab.
      - Select `Create -> Yogi -> Google Sheets Configs -> Configs`
      - Select created file.
      - Set provider for config 
      
      ![image](https://user-images.githubusercontent.com/54948242/212977810-ce313302-a63f-4e1c-9a3f-ba50612cf259.png)
      
    - Add your first config
    
    ![image](https://user-images.githubusercontent.com/54948242/212978995-2a787755-bf6e-446a-95ce-9b9c7854c3b6.png)

     - `Name` - You can give it any name you want. Doesn't affect anything.
     - `Spread Sheet` - Spreadsheet id.
      
     ![image](https://user-images.githubusercontent.com/54948242/212985253-cb37a9a8-0e61-4801-98f6-2206774f86a3.png)
        
     - Sheets
       - `Config Name` - The path where generated config .json file will be saved.
       - `Sheet Id` - The id of the sheet used to load the data.
       
       ![image](https://user-images.githubusercontent.com/54948242/212985300-f1d5c8e0-32ba-4967-a671-df4461604394.png)
       
       - `Range` - Range of sheet used for loading. Examples: 'A1:E1000', '2:1000'. **Leave empty to loading the entire sheet**.
       - `Parser` - The way how to parse data loaded from sheet. Use 'default' parser or create your own.
     - Click `Pull Configs from Google Drive` to load google sheets configs into local json files.
      
   ### Example
   We have sheet config with monsters.
   
   ![image](https://user-images.githubusercontent.com/54948242/212984890-09e9c978-9f2e-4d07-a44a-43a0ed054818.png)
    
   Setup config in Unity.
   
   ![image](https://user-images.githubusercontent.com/54948242/212983257-421da2c4-6338-41a3-b0d0-63ba77ec9a32.png)
   
  After pulling Monsters.json config will be created.
  
  ![image](https://user-images.githubusercontent.com/54948242/212983611-5182d516-5a29-44f6-a25b-becc6b6bde8f.png)

How you can see - with the 'default' parser, the first column is used as the key in json config.

**You can write any unique parser for each table to generate json files of the desired format**

      
  ## Parsers
  WIP
      




