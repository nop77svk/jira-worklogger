# Jira worklogger

<a rel="license" href="http://creativecommons.org/licenses/by-sa/4.0/"><img alt="Creative Commons License" style="border-width:0" src="https://i.creativecommons.org/l/by-sa/4.0/88x31.png" /></a><br />Licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-sa/4.0/">Creative Commons Attribution-ShareAlike 4.0 International License</a>.

## The problem

Do you have to track your work by means of Jira issue worklogs? Is the Jira GUI _clickfest_ approach making your neurotic inner beast emerge on the surface?

## The solution

Meet the scripted Jira worklogging! Give it your worklogs in a CSV file (and your server URI and your user credentials in a config file) and let the automaton do the rest!

## Prerequisites

- .NET 6 run-time installed (for simple, cross-platform build) or no .NET runtime necessary (for self-contained, single-exe, Windows-only build); You choose!
- Jira server (with version 2 REST API) and the "Tempo Timesheets" plugin

## Configuration

The jwl.config file is a simple JSON structure. It can be placed in (and will be read by jwl in the priority order of)
 - "current folder" (as in "where your shell's %CD% or ${PWD} is at the moment")
 - local application data (%USERPROFILE%\AppData\Local)
 - roaming application data (%USERPROFILE%\AppData\Roaming)
 - jwl's "installation" folder

As for the CLI worklogger binary, there are command-line options available as well. Any partial options supplied via CLI will override their respective jwl.config counterparts with the highest priority.

## The input CSV structure

Five columns, data delimited (by default) by a colon:
 - Date (format YYYY-MM-DD accepted only)
 - IssueKey (string)
 - TempoWorklogType - Values are checked against the available values from Jira server on each execution.
 - TimeSpent (string; accepted formats - "HH:MI", "MI", "HH h MI")
 - Comment (string)
