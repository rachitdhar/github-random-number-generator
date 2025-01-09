# GHCRNG
An app that uses the GitHub Contributions Grid from the profile page to Generate Random Numbers

Steps:
- Get the HTML dynamically generated for the profile page.
- Parse the HTML using Regex to get a list of numbers for the contributions grid levels.
- Use SHA256 hashing over a string obtained from the list - from this a random number can be created.
- Write the number to the profile page README and commit it.
