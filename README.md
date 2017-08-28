# Matterhook.NET
Listen for Webhooks and post them to your Mattermost server!

Consume webhooks from all around the web (currently just Github and Discourse) and post them to your favourite Mattermost incoming Webhook

## Deployment

- Clone the repo to your machine (known from this point on as `${RepoDir}`)
- Create the config file in: `${RepoDir}/config/` (Here you will find a `config.json.sample` to give you the framework of the file - More details below)
- Once the config file is created, build and start the bot:
```
cd ${RepoDir}/
docker-compose -f docker-compose.ci.build.yml up
docker-compose up -d --build
```

Note: By default the service listens on port `8080`, however this can be changed by editing `docker-compose.yml`


## Configuration

[Example Config file](https://github.com/PromoFaux/Matterhook.NET/blob/master/config/config.json.sample)

### Discourse Config:

To process Discourse webhooks, point them at `http://<yourdomain>:<port>/DiscourseHook`

```JSON
{
  "DiscourseConfig": {    
    "Secret": "mysecretpassword1",
    "IgnoredTopicTitles": [ "Welcome!", "Backup completed successfully" ],
    "IgnorePrivateMessages": true,
    "MattermostConfig": {
      "WebhookUrl": "https://mattermostserver.com/hooks/asdasdasd",
      "Channel": "atests",
      "Username": "Adam's Test Bot",
      "IconUrl": "https://avatars1.githubusercontent.com/u/3220138"
    }
}
```


![](https://i.imgur.com/CIkgbpA.png)

#### Message types handled:

- :white_check_mark: `post_created`
- :x: Everything Else

There are a few more topic types that I will get around to adding eventually, for now though, I only need the `post_created` ones.

### Github Config:

To process Github webhooks, point them at `http://<yourdomain>:<port>/GithubHook`
```
{  
   "GithubConfig":{  
      "Secret":"sfsdfdsfsdfsdfsd",
      "VerboseCommitMessages":  true, 
      "MattermostConfig": {
        "WebhookUrl": "https://mattermostserver.com/hooks/asdasdasd",
        "Channel": "github-gen",
        "Username": "Gerald II - Just when you thought it was safe to go back in the water...",
        "IconUrl": "https://assets.pi-hole.net/Variants/Logo%20only/GitHubBot.png"
      },
      "RepoList":[  
         {  
            "RepoName":"promofaux/Matterhook.NET.MatterHookClient",
            "MattermostConfig":{  
               "Channel":"matterhook"
            }
         },     
         {  
            "RepoName":"promofaux/Matterhook.NET",
            "MattermostConfig":{  
               "Channel":"matterhook",
               "Username": "BEST BOT"
            }
         }
      ]
   }
}
```

A `GithubConfig` is set up with a default `MattermostConfig`. `RepoList[]` is not needed to recieve hooks, just point your repo at the running instance of Matterhook.NET and you're good to go! If you have multiple repos that you wish to post to separate channels (or server!), you can do so by adding a `RepoList[]` (see above), any Repo in the list can override any setting in the default `MattermostConfig` by adding them to this object.

![](https://i.imgur.com/SZ8lZ7J.png)

#### Events handled:

- :white_check_mark: `pull_request`
  - :white_check_mark: `opened`
  - :white_check_mark: `labeled`
  - :white_check_mark: `unlabeled`
  - :white_check_mark: `closed`
  - :white_check_mark: `assigned`
  - :white_check_mark: `unassigned`
- :white_check_mark: `issue`
  - :white_check_mark: `opened`
  - :white_check_mark: `closed`
  - :white_check_mark: `reopened`
  - :white_check_mark: `labeled`
  - :white_check_mark: `unlabeled`
  - :white_check_mark: `assigned`
  - :white_check_mark: `unassigned`
- :white_check_mark: `issue_comment`
  - :white_check_mark: `created`
  - :x: `edited` (this one gets annoying!)
- :x: `repository`
  - :x: `created` (planned)
- :white_check_mark: `create`
  - :white_check_mark: `branch`
  - :white_check_mark: `tag`
- :white_check_mark: `delete`
  - :white_check_mark: `branch`
  - :white_check_mark: `tag`
- :white_check_mark: `pull_request_review_comment`
  - :white_check_mark: `created`
- :white_check_mark: `push` (commits only)
- :white_check_mark: `commit_comment`
  - :white_check_mark: `created`


I've not managed to do everything yet, just wanted to get the bare bones in to make it a functional piece of software to use for my own use. (Typing out the classes for JSON Serializing was a long and arduous process.. and yes, I realised near the end that you can just `Edit>Paste Special>Paste JSON As Classes`!)

## Future plans /todos:
- Process more of the Github events. Feel free to help out with that one, at this stage it's just a typing excersise!
- MOAR WEBHOOKS.

## Attributions:

HTML from the `cooked` portion of the Discourse webhook parsed with [ReverseMarkdown](https://github.com/mysticmind/reversemarkdown-net)
