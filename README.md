# Matterhook.NET
Listen for Webhooks and post them to your Mattermost server!

Consume webhooks from all around the web (currently just Github and Discourse) and post them to your favourite Mattermost incoming Webhook

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/67972966fee84da1b4f53d5a9f79e8f9)](https://www.codacy.com/app/promofaux/Matterhook.NET?utm_source=github.com&utm_medium=referral&utm_content=PromoFaux/Matterhook.NET&utm_campaign=badger)
[![Docker Build Status](https://img.shields.io/docker/build/promofaux/matterhook.net.svg)](https://hub.docker.com/r/promofaux/matterhook.net/builds/) [![Docker Stars](https://img.shields.io/docker/stars/promofaux/matterhook.net.svg)](https://hub.docker.com/r/promofaux/matterhook.net/) [![Docker Pulls](https://img.shields.io/docker/pulls/promofaux/matterhook.net.svg)](https://hub.docker.com/r/promofaux/matterhook.net/) 

## Deployment
Recommended - Use pre-built container:
- Create a directory to store the bot's config file, e.g `/opt/bot/Mattermost.NET` (`${YOUR_DIRECTORY}`)
- Create the config file in `${YOUR_DIRECTORY}`. See [Example Config file](https://github.com/PromoFaux/Matterhook.NET/blob/master/config/config.json.sample) for details, or read below.
- `docker run -d --restart=always -v ${YOUR_DIRECTORY}/:/config/ -p ${YOUR_PORT}:80 --name Matterhook.NET promofaux/matterhook.net`

`${YOUR_PORT}` is the port you wish Matterhook.NET to listen on.


Alternative - build the container yourself:
- Clone the repo to your machine (known from this point on as `${RepoDir}`)
- Create the config file in: `${RepoDir}/config/` (Here you will find a `config.json.sample` to give you the framework of the file - More details below)
- Once the config file is created, build and start the bot:
```
cd ${RepoDir}/
docker-compose -f docker-compose.ci.build.yml up
docker-compose up -d --build
```

Note: Change the listening port in `docker-compose.yml` if you want it to listen on a port other than 8080, that's just what I use for development


## Configuration

[Example Config file](https://github.com/PromoFaux/Matterhook.NET/blob/master/config/config.json.sample)

In order to cut down on log spam, incoming payloads that are succesfully posted to Mattermost are not written to the console. If you wish to log everything, you can add `"LogOnlyErrors": false` to the desired config section.

### Discourse Config:

To process Discourse webhooks, point them at `http(s)://<yourdomain>:<port>/DiscourseHook`

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

To process Github webhooks, point them at `http(s)://<yourdomain>:<port>/GithubHook`
```
{  
   "GithubConfig":{  
      "Secret":"sfsdfdsfsdfsdfsd",      
      "DefaultMattermostConfig": {
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

### Docker Hub Config:

To Process Docker Hub Webhooks, point them at `http(s)://<yourdomain>:<port>/DockerHubHook`

```
{  
   "DockerHubConfig":{  
      "DefaultMattermostConfig":{  
         "WebhookUrl":"https://mattermostserver.com/hooks/asdasdasd",
         "Username":"Docker Hub Bot",
         "IconUrl":"https://avatars0.githubusercontent.com/u/5429470",
	     "Channel": "docker-news"
      },
      "RepoList":[  
         {  
            "RepoName":"promofaux/Matterhook.NET",
            "MattermostConfig":{  
               "Channel":"matterhook"
            }
         }
      ]
   }
}
```

![](https://i.imgur.com/BMkHD9h.png)

## Future plans /todos:
- Process more of the Github events. Feel free to help out with that one, at this stage it's just a typing excersise!
- MOAR WEBHOOKS.

## Attributions:

HTML from the `cooked` portion of the Discourse webhook parsed with [ReverseMarkdown](https://github.com/mysticmind/reversemarkdown-net)
