const express = require('express');
const https = require('https');
const bodyParser = require('body-parser')

const app = express();

app.use(bodyParser.urlencoded({ extended: false }))
app.use(bodyParser.json())

app.post('/', (req, res) => {
    const body = req.body;

    if (!body || !body.urls || !Array.isArray(body.urls)) {
        res.status(400);
    }

    const urls = body.urls;

    let responses = {};
    let requests = [];

    for (let url of urls) {
        console.log('Querying', url)
        requests.push(new Promise((resolve, reject) => {
            https.get(url, (response) => {
                let data = '';

                response.on('data', chunk => {
                    data += chunk;
                });

                response.on('end', () => {
                    responses[url] = data;
                    resolve()
                });
            }).on('error', err => {
                console.log('Error with', url, err);
                reject(err);
            });
        }));
    }

    Promise.all(requests)
        .then(() => res.send(responses))
        .catch(err => res.status(500).send(err));
});

app.listen(80, () => {
    console.log('Server running')
});