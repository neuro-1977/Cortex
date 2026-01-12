
client.on('message', async message => {
    if (message.author.bot) return;

    if (message.content === '!joke') {
        message.channel.send("**Mao**: Why did the cat sit on the computer? To keep an eye on the mouse! :3");
    } else if (message.content === '!scold') {
        message.channel.send("**Isla**: Your existence is an error. Go away.");
    } else if (message.content === '!logtest') {
        const channel = message.guild.channels.cache.find(c => c.name === 'general-chat');
        if (channel) channel.send('Log test successful in general-chat!');
    } else if (message.content.startsWith('!purge ')) {
        const count = parseInt(message.content.split(' ')[1]) || 0;
        const channel = message.channel;
        if (channel && count > 0) {
            let remaining = count;
            while (remaining > 0) {
                const limit = remaining > 100 ? 100 : remaining;
                const fetched = await channel.messages.fetch({ limit: limit });
                if (fetched.size === 0) break;
                // Pass true to filter out messages older than 14 days to prevent crash
                await channel.bulkDelete(fetched, true);
                remaining -= fetched.size;

                if (fetched.size < limit) break;
            }
        }
    } else if (message.mentions.has(client.user)) {
        const rand = Math.random();
        if (rand < 0.4) {
            message.channel.send("**Isla**: Ugh, what do you want? Go away.");
        } else if (rand < 0.8) {
            message.channel.send("**Mao**: Heya! Wanna play a game? :3");
        } else {
            message.channel.send("**Isla**: Don't touch me.");
            message.channel.send("**Mao**: Aww, don't be like that Isla! Hi human!");
        }
    }
});
