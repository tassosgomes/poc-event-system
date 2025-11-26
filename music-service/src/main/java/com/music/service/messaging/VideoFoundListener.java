package com.music.service.messaging;

import com.music.service.domain.event.VideoFoundEvent;
import com.music.service.service.SongService;
import io.github.springwolf.core.asyncapi.annotations.AsyncListener;
import io.github.springwolf.core.asyncapi.annotations.AsyncOperation;
import io.github.springwolf.plugins.amqp.asyncapi.annotations.AmqpAsyncOperationBinding;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
@Slf4j
public class VideoFoundListener {

    private final SongService songService;

    @RabbitListener(queues = "${app.rabbitmq.queue.video-found}")
    @AsyncListener(
            operation = @AsyncOperation(
                    channelName = "music.video-found",
                    description = "Event consumed when a video metadata is available for a song"
            )
    )
    @AmqpAsyncOperationBinding()
    public void handleVideoFound(VideoFoundEvent event) {
        log.info("Received VideoFoundEvent for song {}", event.getSongId());
        songService.processVideoFoundEvent(event);
    }
}
