// backend/server.js
const express = require('express');
const cors = require('cors');
const { GoogleGenAI, Modality } = require('@google/genai');
require('dotenv').config();

const app = express();
app.use(cors());
app.use(express.json());

const PORT = 3000;

const ai = new GoogleGenAI({
    apiKey: process.env.GEMINI_API_KEY,
});

// WAV oluşturma fonksiyonları (değişiklik yok)
function createWavHeader(dataLength, options) {
    const { numChannels, sampleRate, bitsPerSample } = options;
    const byteRate = sampleRate * numChannels * bitsPerSample / 8;
    const blockAlign = numChannels * bitsPerSample / 8;
    const buffer = Buffer.alloc(44);
    buffer.write('RIFF', 0); buffer.writeUInt32LE(36 + dataLength, 4); buffer.write('WAVE', 8);
    buffer.write('fmt ', 12); buffer.writeUInt32LE(16, 16); buffer.writeUInt16LE(1, 20);
    buffer.writeUInt16LE(numChannels, 22); buffer.writeUInt32LE(sampleRate, 24);
    buffer.writeUInt32LE(byteRate, 28); buffer.writeUInt16LE(blockAlign, 32);
    buffer.writeUInt16LE(bitsPerSample, 34); buffer.write('data', 36);
    buffer.writeUInt32LE(dataLength, 40);
    return buffer;
}
function parseMimeType(mimeType) {
    const params = mimeType.split(';').map(s => s.trim());
    const options = { numChannels: 1, bitsPerSample: 16, sampleRate: 24000 };
    for (const param of params) {
        if (param.startsWith('rate=')) options.sampleRate = parseInt(param.substring(5), 10);
    }
    return options;
}

// Angular'ın istek atacacağı API endpoint'i
app.post('/api/generate-voice', async (req, res) => {
    const { prompt } = req.body;
    if (!prompt) return res.status(400).json({ error: 'Prompt is required' });

    console.log(`İstek alındı: "${prompt}"`);

    try {
        const audioParts = [];
        let mimeType = '';
        let streamFinishedTimer = null; // Zamanlayıcımız

        // Fonksiyonu dışarı taşıdık ki hem timer'dan hem callback'ten çağrılabilsin
        const finishStream = () => {
            if (res.headersSent) return; // Yanıt zaten gönderildiyse bir şey yapma
            
            clearTimeout(streamFinishedTimer); // Zamanlayıcıyı temizle

            console.log('Ses akışı tamamlandı veya zaman aşımına uğradı. WAV dosyası oluşturuluyor.');
            if (audioParts.length > 0) {
                const rawData = Buffer.concat(audioParts);
                const options = parseMimeType(mimeType);
                const header = createWavHeader(rawData.length, options);
                const wavBuffer = Buffer.concat([header, rawData]);

                res.setHeader('Content-Type', 'audio/wav');
                res.send(wavBuffer);
            } else {
                res.status(500).json({ error: 'Gemini API\'den ses verisi alınamadı.' });
            }
        };

        const session = await ai.live.connect({
            model: 'models/gemini-1.5-flash',
            config: {
                responseModalities: [Modality.AUDIO],
                speechConfig: { voiceConfig: { prebuiltVoiceConfig: { voiceName: 'Zephyr' } } }
            },
            callbacks: {
                onmessage: (message) => {
                    // Güvenilir olmasa da turnComplete gelirse akışı bitir
                    if (message.serverContent?.turnComplete) {
                        finishStream();
                        session.close();
                        return;
                    }
                    
                    const part = message.serverContent?.modelTurn?.parts?.[0];
                    if (part?.inlineData) {
                        // Yeni ses paketi geldikçe zamanlayıcıyı sıfırla
                        clearTimeout(streamFinishedTimer);
                        streamFinishedTimer = setTimeout(finishStream, 1000); // 1 saniye bekle

                        if (!mimeType) mimeType = part.inlineData.mimeType;
                        audioParts.push(Buffer.from(part.inlineData.data, 'base64'));
                    }
                },
                onerror: (e) => {
                    console.error('Gemini API Hatası:', e);
                    if (!res.headersSent) {
                        res.status(500).json({ error: 'Gemini API ile iletişimde bir hata oluştu.' });
                    }
                    session.close();
                }
            }
        });
        
        await session.sendClientContent({ turns: [{ role: 'user', parts: [{ text: prompt }] }] });

    } catch (error) {
        console.error('Bağlantı Hatası:', error);
        res.status(500).json({ error: 'Gemini\'ye bağlanılamadı.' });
    }
});

app.listen(PORT, () => {
    console.log(`Ses üretme proxy sunucusu http://localhost:${PORT} adresinde çalışıyor`);
});