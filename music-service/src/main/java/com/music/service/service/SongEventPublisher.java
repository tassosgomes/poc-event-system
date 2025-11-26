package com.music.service.service;

import com.music.service.domain.event.SongCreatedEvent;
import io.github.springwolf.core.asyncapi.annotations.AsyncOperation;
import io.github.springwolf.core.asyncapi.annotations.AsyncPublisher;
import io.github.springwolf.plugins.amqp.asyncapi.annotations.AmqpAsyncOperationBinding;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
@Slf4j
public class SongEventPublisher {

    private final RabbitTemplate rabbitTemplate;

    @Value("${app.rabbitmq.exchange}")
    private String exchange;

    @Value("${app.rabbitmq.routing-key.song-created}")
    private String routingKey;

    @AsyncPublisher(
            operation = @AsyncOperation(
                    channelName = "music.song-created",
                    description = "Event published when a new song is created"
            )
    )
    @AmqpAsyncOperationBinding()
    public void publishSongCreatedEvent(SongCreatedEvent event) {
        log.info("Publishing SongCreatedEvent: {}", event);
        rabbitTemplate.convertAndSend(exchange, routingKey, event);
    }
}
