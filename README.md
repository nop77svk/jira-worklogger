# Jira Worklogger

<a rel="license" href="http://creativecommons.org/licenses/by-sa/4.0/"><img alt="Creative Commons License" style="border-width:0" src="https://i.creativecommons.org/l/by-sa/4.0/88x31.png" /></a><br />Licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-sa/4.0/">Creative Commons Attribution-ShareAlike 4.0 International License</a>.

## The problem

Do you have to track your work by means of Jira issue worklogs? Is the Jira GUI _clickfest_ approach making your neurotic inner beast emerge on the surface?

## The solution

Meet the scripted Jira worklogging! Give it your worklogs in a CSV file (and your server URI and your user credentials in a config file) and let the automaton do the rest!

## Prerequisites

- .NET 6 run-time installed (for simple, cross-platform build) or no .NET runtime necessary (for self-contained, single-exe, Windows-only build); You choose!
- Jira server
  - "vanilla" Jira server support: ✔️ (version 2 REST API)
  - "Tempo Timesheets" plugin support: ✔️ (version 4 REST API)
  - "ICTime" plugin support: ✔️ (version 1.0 REST API)

## Configuration

The jwl.config file is a simple JSON structure. It can be placed in (and will be read by jwl in the priority order of)
- "current folder" (as in "where your shell's <code>%CD%</code> or <code>${PWD}</code> is at the moment")
- local application data (<code>%USERPROFILE%\AppData\Local</code>)
- roaming application data (<code>%USERPROFILE%\AppData\Roaming</code>)
- jwl's "installation" folder

As for the CLI worklogger binary, there are command-line options available as well. Any partial options supplied via CLI will override their respective jwl.config counterparts with the highest priority.

### "ServerClass" setting

Available values are:
- Vanilla
- TempoTimeSheets
- ICTime

## The input CSV structure

Five columns, data delimited (by default) by a colon:
 - <code>Date</code> (string) - worklog day date (valid formats: <code>YYYY-MM-DD</code>, <code>YYYY/MM/DD</code>, <code>DD.MM.YYYY</code>, all with optional <code> HH:MI:SS</code> part)
 - <code>IssueKey</code> (string) - Jira issue key (<code>SOMEPROJECT-1234</code> and the likes)
 - <code>Activity</code> (string) - Tempo Timesheets worklog type or ICTime activity; values are remapped 
 - <code>TimeSpent</code> (string) - time to be logged for the Jira issue and the date (valid formats: <code>HH:MI</code>, <code>MI</code>, <code>HH h MI</code>, <code>HH h MI m</code>)
 - <code>Comment</code> (string) - optional worklog comment
