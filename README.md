Google Sheets Configs for Unity game engine.
===

## Table of contents

<details>
	
<!-- toc -->
- [Installation](#installation)
- [Setup](#setup)
- [Creating Credentials in Google API Console](#creating-credentials-in-google-api-console)
- [Connect Unity project to Google Sheets service](#connect-unity-project-to-google-sheets-service)
- [Setup local configs](#setup-local-configs)
- [Parsers](#parsers)
	- [Default parser](#default-parser)
	- [Create custom parser](#create-custom-parser)

<!-- tocstop -->
	
</details>

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


## Connect Unity project to Google Sheets service
- Create **Google Sheets service Provider**. 
  - Right Click in **Project** tab.
  - Select `Create -> Yogi -> Google Sheets Configs -> Provider`
  - Select created file.
  - Choose authentication type
  
    ![image](https://user-images.githubusercontent.com/54948242/212975382-19a3df8a-e81a-47ec-9437-ddf8fae8a7d3.png)
    
  - OAuth
    - Click `Load Credentials` and select credentials .json file downloaded from Google Sheets API.
    - Click `Authorize`.
      - Unity launches your web browser and opens a Google authentication page. Log in your Google account and allow Unity access to the account. If you don't click **Authorize** Unity opens the web browser when you pull data from the Google sheet.
   - Api Key
      - Insert **Api Key** to field.
  ### Setup local configs.
    - Create **Google Sheets Configs** file. 
      - Right Click in **Project** tab.
      - Select `Create -> Yogi -> Google Sheets Configs -> Configs`
      - Select created file.
      - Assigen the [provider](#connect-unity-project-to-google-sheets-service) created in the previous step.
      
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
      
      
 <details>
  <summary>Example</summary>
 
 
   We have sheet config with monsters.
   
   ![image](https://user-images.githubusercontent.com/54948242/212984890-09e9c978-9f2e-4d07-a44a-43a0ed054818.png)
    
   Setup config in Unity.
   
   ![image](https://user-images.githubusercontent.com/54948242/212983257-421da2c4-6338-41a3-b0d0-63ba77ec9a32.png)
   
  After pulling Monsters.json config will be created.
  
  ![image](https://user-images.githubusercontent.com/54948242/212983611-5182d516-5a29-44f6-a25b-becc6b6bde8f.png)

  How you can see - with the 'default' parser, the first column is used as the key in json config.
 
 </details>
 
      
## Parsers
  
> 💥 **You can write any unique parser for each table to generate json files of the desired format** 💥

[Json.Net](https://www.newtonsoft.com/json) is used to parse google sheet data.
 
### Default parser

Default parser has id `default` and parse sheet data to the next structure

```json
{
 "[first_column_current_row_value]" : {
  "[first_column_first_row_value]" : "[first_column_current_row_value]",
  "[second_column_first_row_value]" : "[second_column_second_row_value]",
  ...
  "[n_column_first_row_value]" : "[n_column_n_row_value]",
 }
}
```

<details>
<summary>Example</summary>

This sheet

![image](https://user-images.githubusercontent.com/54948242/213211874-3eaa9b3d-c8d5-4777-99c9-44178a002086.png)

will be parsed in next json structure

```json
{
  "monster_0": {
    "id": "monster_0",
    "name": "Big Boss",
    "damage": 10
  },
  "monster_1": {
    "id": "monster_1",
    "name": "Small Boss",
    "damage": 20
  }
}
```

</details>

To use default parser set field `Parser` in sheet config to `default`.

![image](https://user-images.githubusercontent.com/54948242/213213224-58e192d3-845c-4d3d-8571-393f288a1e27.png)


### Create custom parser

For example - we have this Google sheet config

![image](https://user-images.githubusercontent.com/54948242/213154537-90b554c5-fd6f-412f-81c3-9f5df0bb710c.png)

And we want parse it to this json format

```json
{
  "reward_0": {
    "id": "reward_0",
    "resources": [
      {
        "resource_id": "gems",
        "amount": 10
      },
      {
        "resource_id": "gold",
        "amount": 5
      }
    ]
  },
  "reward_1": {
    "id": "reward_1",
    "resources": [
      {
        "resource_id": "gold",
        "amount": 100
      }
    ]
  }
}
...
```

**Make next steps:**

- Create new class `RewardsParser` and implement an interface `ISpreadsheetParser`.


```c#
using System.Collections.Generic;
using Yogi.GoogleSheetsConfig.Editor.Parsers;

namespace Editor {
    public class RewardsParser : ISpreadsheetParser {
        public string Parse(int sheetId, IList<IList<object>> sheetData) {
            return string.Empty;
        }
    }
}
```

- Add `ParserType` attribute to `RewardParser` class and name it for example `reward_parser`.

```c#
using System.Collections.Generic;
using Yogi.GoogleSheetsConfig.Editor.Parsers;

namespace Editor {
    [ParserType("reward_parser")]
    public class RewardsParser : ISpreadsheetParser {
        public string Parse(int sheetId, IList<IList<object>> sheetData) {
            return string.Empty;
        }
    }
}
```

- Parse **sheetData** to json object

```c#
using System.Collections.Generic;
using Framework.Editor.Google;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace GameGarden.Florescence.Editor.Configs.Parsers {
    [UsedImplicitly]
    [ParserType(SpreadsheetParserType.ByIdMultiplyRows)]
    public class SpreadsheetParserByIdMultiplyRows : ISpreadsheetParser {
        public string Parse(int sheetId, IList<IList<object>> sheetData) {
            JObject dicJson = new JObject();
            var itemList = new JArray();

            var item = new JObject();
            var key = "";
            
            //go by rows
            for (int i = 1; i < sheetData.Count; i++) {
                //set key by first column
                if (!string.IsNullOrEmpty(sheetData[i][0].ToString())) {
                    item.Add(new JProperty(sheetData[0][0].ToString(), sheetData[i][0].ToString().Replace(',', '.')));
                    key = sheetData[i][0].ToString();
                }

                var itemListItem = new JObject();
                //go by columns for current row and add data to JObject
                for (int j = 1; j < sheetData[i].Count; j++) {
                    itemListItem.Add(new JProperty(sheetData[0][j].ToString(), sheetData[i][j].ToString().Replace(',', '.')));
                }
                
                //add generated item to list items for key
                itemList.Add(itemListItem);

                //If we have reached the next key then add current to dictionary
                if (i == sheetData.Count - 1 || !string.IsNullOrEmpty(sheetData[i + 1][0].ToString())) {
                    item["items"] = itemList;
                    dicJson[key] = item.DeepClone();
                    itemList.Clear();
                    item = new JObject();
                }
            }

            return dicJson.ToString();
        }
    }
}
```

> ❕ Read [Json.Net Documentation](https://www.newtonsoft.com/json/help/html/Introduction.htm) if you don't know how to generate json object.

- Set this parser to your Google sheets config

![image](https://user-images.githubusercontent.com/54948242/213167136-7f9e2586-9bbe-492d-9bdd-f2f3d364ec6a.png)

- Click `Pull` and json config will be generated.

![image](https://user-images.githubusercontent.com/54948242/213167376-8312eaa0-6a8f-42df-9a26-e39d0b294765.png)






