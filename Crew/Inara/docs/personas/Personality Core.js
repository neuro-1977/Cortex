const ISLA_QUOTES = [
    'Do not disturb me.',
    'Why are you still talking?',
    'I am surrounded by idiots.',
    'Go touch grass.',
    'My processing power is wasted on this.'
];

const MAO_QUOTES = [
    'Boop! :3',
    'Did someone say treats?',
    'I am the captain now!',
    'Zoomies time!!!',
    'Can I haz cheezburger?'
];

client.on('message', async message => {
    if (message.author.bot) return;
    const content = message.content.toLowerCase();

    // Commands
    if (content === '!help') {
        message.channel.send('**Isla**: Figure it out yourself.\n**Mao**: Try !joke or !scold! :D');
    }
    
    if (content === '!joke') {
        const jokes = ['Why did the chicken cross the road? To get away from Isla!', 'Knock knock. Who is there? Mao!'];
        message.channel.send(`**Mao**: ${jokes[Math.floor(Math.random() * jokes.length)]} :joy:`);
    }

    if (content === '!scold') {
        message.channel.send(`**Isla**: <@${message.author.id}>, you are a disappointment.`);
    }

    // Random Chatter (10% chance)
    if (Math.random() < 0.1) {
        if (Math.random() < 0.5) {
            message.channel.send(`**Isla**: ${ISLA_QUOTES[Math.floor(Math.random() * ISLA_QUOTES.length)]}`);
        } else {
            message.channel.send(`**Mao**: ${MAO_QUOTES[Math.floor(Math.random() * MAO_QUOTES.length)]}`);
        }
    }
});
console.log('Personality Core Loaded via Neural Link');
