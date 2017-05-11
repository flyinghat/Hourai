# Hourai - Discord Bot
Hourai is a highly configurable, general purpose Discord bot.

## Getting Started
To add Hourai to your Discord server,

## Modules
Hourai has a large number of commands. These commands are organized into the
following modules:
 * [Standard](/wiki/modules/standard/) - A standard set of commands
 * [Admin](/wiki/modules/admin/) - Adminsitrative commands for server moderation
 * [Feeds](/wiki/modules/feeds/) - Automatic feeds
 * [Touhou](/wiki/modules/touhou/) - Commands related to [*Touhou
   Project*](https://en.touhouwiki.net/wiki/Touhou_Project)
 * [Misc](/wiki/modules/misc/) - Miscellaneous fun commands

## Usage
Hourai responds to commands sent as chat messages. All commands come in the
format `<prefix><command> <arguments>`.  By default, the prefix is `~`.
The command prefix for Hourai can be configured via the `config prefix <prefix>`
command (i.e. `~config prefix .` will change the prefix, for the entire server
to `.`).

Hourai provides a handy `~help` command that provides in-chat help for any and
all commands.

### Server Moderation
Hourai provides many powerful moderation tools. There are the basic server
manipulation commands (`role add`, `channel rename`, `prune`, etc.) found in many
Discord bots. Howver, many of these commands have multiple subcommands that
provide more control on how these commands are used. For example, `~prune` will
delete X messages from the current channel and the subcommand `~prune embed`
will do the same, but only the messages that have attachments or embeds.

Many of these commands allow taking more than one target as arguments. For
example `~ban @user1 @user2 @user3` will server ban all 3 of them
simultaneously.

All of these commands are require specific permissions to run, both for the bot
and the user running the command. For example, all `role <x>` commands like
`~role add` requires the Manage Roles permission for both Hourai and the user
using the command.

All moderation changes and commands are automatically logged, this log can be
retrieved via the `~modlog` command.

For more information, see the [Admin](/wiki/modules/admin/)

### Server Announcements
Hourai can anounce member joins, leaves, bans via the `~announce join`,
`~announce leave`, `~announce ban` commands.

Hourai can also notify when server members have started or stopped streaming via
the  `~announce stream` command.

See [Feeds](/wiki/modules/feeds/) for more information.

### Automatic Feeds
Hourai is capable of providing automatic feeds, integrating into other services.
Currently only reddit feeds are supported: Hourai will automatically make posts
to a configured channel when new posts are found on specified subreddits. For
more information check the [Feeds](/wiki/modules/feeds/) documentation page.

## Configuration
TODO: Create a web UI for configuring the bot.

Hourai supports a per-server custom configuration that allows the following:
 * Automatically executing commands in response to an assortment of events.
 * Creating aliases to existing commands.
 * More to come!

Custom configs can be uploaded via the `~config upload` command.

Custom command and responses can be created, edited, and removed via the
`~command` command.

For examples of these configs and more information see [here](/wiki/config/).

## Contributing
Hourai is [open source](https://github.com/james7132/Hourai).
