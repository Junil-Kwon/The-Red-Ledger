// --- Prologue: The Meeting of Three ---
# speaker: 0
We're finally all here. Everyone, are you ready for this mission?

# speaker: 1
I'm good to go. Finished checking all my gear. But hey... why has 2 been so quiet?

# speaker: 2
(My comrades are staring at me. How should I respond?)

// 3 Choices (Protagonist's reaction)
+ [Of course I'm ready!]
    # speaker: 2
    Don't worry. I've never felt better.
    # speaker: 0
    Good. I like that energy. I knew I could count on you.
    -> discussion
+ [I'm a bit nervous.]
    # speaker: 2
    To be honest, I'm a little shaky. Do you think we can pull this off?
    # speaker: 1
    What's wrong? That's not like you. We've got your back, don't forget that!
    -> discussion
+ [Just lost in thought.]
    # speaker: 2
    I was just visualizing our infiltration route one more time.
    # speaker: 0
    Always the cautious one. Shall we take a look at the map for a final check then?
    -> discussion

=== discussion ===
# speaker: 1
Alright, we've reached the entrance. Who's going to take the lead from here?

+ [I'll lead the way. (2)]
    # speaker: 2
    I'll go first. You guys cover my back.
    # speaker: 0
    It's going to be dangerous... Alright. Be careful, 2.
    -> final_gate
+ [0, you take command.]
    # speaker: 2
    0, your judgment is the sharpest. You open the path for us.
    # speaker: 0
    Trust me. I'll lead us through the safest route possible.
    -> final_gate
+ [I'll leave it to 1.]
    # speaker: 2
    1, we need your agility. Can you handle the traps?
    # speaker: 1
    Heh, is it finally my turn? I'll have it cleared in the blink of an eye!
    -> final_gate

=== final_gate ===
# speaker: 0
We're at the gate. Once we open this, there's no turning back. Is everyone's resolve ready?

+ [No time to hesitate.]
    # speaker: 2
    Let's open the gate right now. We're going to win this.
    # speaker: 1
    That's the 2 I know! Alright, let's go!
    -> DONE
+ [Let's all come back safe.]
    # speaker: 2
    The battle is important, but making sure no one gets hurt is my priority.
    # speaker: 0
    Right. We're coming back together, all three of us. That's a promise.
    -> DONE
+ [Wait... let me catch my breath.]
    # speaker: 2
    Phew... Okay. Now I'm ready. Open it!
    # speaker: 1
    Great, let's move out!
    -> DONE