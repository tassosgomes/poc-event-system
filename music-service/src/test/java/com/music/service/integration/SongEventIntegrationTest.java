package com.music.service.integration;

import com.music.service.domain.event.SongCreatedEvent;
import com.music.service.dto.SongRequest;
import com.music.service.service.SongService;
import org.junit.jupiter.api.Test;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.DynamicPropertyRegistry;
import org.springframework.test.context.DynamicPropertySource;
import org.testcontainers.containers.PostgreSQLContainer;
import org.testcontainers.containers.RabbitMQContainer;
import org.testcontainers.junit.jupiter.Container;
import org.testcontainers.junit.jupiter.Testcontainers;

import java.time.LocalDate;

import static org.assertj.core.api.Assertions.assertThat;

@SpringBootTest
@Testcontainers
public class SongEventIntegrationTest {

    @Container
    static RabbitMQContainer rabbitMQ = new RabbitMQContainer("rabbitmq:3.12-management");

    @Container
    static PostgreSQLContainer<?> postgres = new PostgreSQLContainer<>("postgres:16-alpine");

    @DynamicPropertySource
    static void configureProperties(DynamicPropertyRegistry registry) {
        registry.add("spring.rabbitmq.host", rabbitMQ::getHost);
        registry.add("spring.rabbitmq.port", rabbitMQ::getAmqpPort);
        registry.add("spring.rabbitmq.username", rabbitMQ::getAdminUsername);
        registry.add("spring.rabbitmq.password", rabbitMQ::getAdminPassword);

        registry.add("spring.datasource.url", postgres::getJdbcUrl);
        registry.add("spring.datasource.username", postgres::getUsername);
        registry.add("spring.datasource.password", postgres::getPassword);
    }

    @Autowired
    private SongService songService;

    @Autowired
    private RabbitTemplate rabbitTemplate;

    @Test
    void createSong_ShouldPublishEventToRabbitMQ() {
        // Arrange
        SongRequest request = new SongRequest();
        request.setTitle("Integration Test Song");
        request.setArtist("Integration Artist");
        request.setReleaseDate(LocalDate.now());
        request.setGenre("Integration Genre");

        // Act
        songService.createSong(request);

        // Assert
        // We can consume from the queue to verify
        Object message = rabbitTemplate.receiveAndConvert("music.song-created", 5000);
        assertThat(message).isNotNull();
        assertThat(message).isInstanceOf(SongCreatedEvent.class);
        SongCreatedEvent event = (SongCreatedEvent) message;
        assertThat(event.getTitle()).isEqualTo("Integration Test Song");
    }
}
