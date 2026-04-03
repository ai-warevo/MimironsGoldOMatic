# Original request

Update the documentation, using all *.md files (except those in .cursor/** and docs/prompts/**)

Change to the gold payout process:
Instead of being paid instantly, the user is added to a list. We also need to add a visual roulette so that every 5 minutes it selects a user from the list as the winner, and the streamer can send them gold. The remaining viewers remain on the list and are not removed. The minimum number of participants for the roulette is one. You can speed up the roulette by spending channel points on a special new reward, "Switch to instant spin." Important clarification: gold can only be sent to a recipient who is online and confirms receipt of the gold. To do this, the winner must send the streamer a private message in-game specifically with the word "!twgold." The addon should intercept this message and notify the utility (the intermediary between the game and our server), and the server will then confirm the gold has been sent.
