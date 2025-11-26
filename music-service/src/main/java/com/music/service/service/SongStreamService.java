package com.music.service.service;

import com.music.service.dto.SongResponse;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Service;
import org.springframework.web.servlet.mvc.method.annotation.SseEmitter;

import java.io.IOException;
import java.time.Duration;
import java.util.Set;
import java.util.concurrent.CopyOnWriteArraySet;

@Service
@Slf4j
public class SongStreamService {

    private static final long DEFAULT_TIMEOUT_MS = Duration.ofMinutes(30).toMillis();
    private final Set<SseEmitter> emitters = new CopyOnWriteArraySet<>();

    public SseEmitter registerEmitter() {
        SseEmitter emitter = new SseEmitter(DEFAULT_TIMEOUT_MS);
        emitters.add(emitter);

        emitter.onCompletion(() -> emitters.remove(emitter));
        emitter.onTimeout(() -> emitters.remove(emitter));
        emitter.onError(ex -> {
            emitters.remove(emitter);
            log.debug("Removing failed SSE emitter", ex);
        });

        return emitter;
    }

    public void broadcastSongUpdate(SongResponse songResponse) {
        emitters.forEach(emitter -> {
            try {
                emitter.send(SseEmitter.event()
                        .name("song-update")
                        .data(songResponse, MediaType.APPLICATION_JSON));
            } catch (IOException ex) {
                emitters.remove(emitter);
                emitter.completeWithError(ex);
                log.debug("Failed to send SSE event, emitter removed", ex);
            }
        });
    }
}
