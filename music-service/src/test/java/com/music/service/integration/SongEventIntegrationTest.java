package com.music.service.integration;

import com.music.service.domain.Song;
import com.music.service.domain.event.SongCreatedEvent;
import com.music.service.dto.SongRequest;
import com.music.service.repository.SongRepository;
import com.music.service.service.SongService;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.ArgumentCaptor;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.test.context.TestPropertySource;

import java.time.LocalDate;
import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.verify;

@SpringBootTest
@TestPropertySource(properties = {
        "spring.datasource.url=jdbc:h2:mem:music-db;DB_CLOSE_DELAY=-1;MODE=PostgreSQL",
        "spring.datasource.driver-class-name=org.h2.Driver",
        "spring.datasource.username=sa",
        "spring.datasource.password=",
    "spring.jpa.hibernate.ddl-auto=create-drop",
    "spring.rabbitmq.listener.simple.auto-startup=false",
    "spring.rabbitmq.listener.direct.auto-startup=false"
})
class SongEventIntegrationTest {

    @Autowired
    private SongService songService;

    @Autowired
    private SongRepository songRepository;

    @MockBean
    private RabbitTemplate rabbitTemplate;

    @BeforeEach
    void cleanDatabase() {
        songRepository.deleteAll();
    }

    @Test
    void createSong_ShouldPersistAndPublishEvent() {
        SongRequest request = new SongRequest();
        request.setTitle("Integration Test Song");
        request.setArtist("Integration Artist");
        request.setReleaseDate(LocalDate.now());
        request.setGenre("Integration Genre");

        songService.createSong(request);

        List<Song> songs = songRepository.findAll();
        assertThat(songs).hasSize(1);

        ArgumentCaptor<SongCreatedEvent> eventCaptor = ArgumentCaptor.forClass(SongCreatedEvent.class);
        verify(rabbitTemplate).convertAndSend(eq("music.events"), eq("song.created"), eventCaptor.capture());
        SongCreatedEvent event = eventCaptor.getValue();
        assertThat(event.getTitle()).isEqualTo("Integration Test Song");
        assertThat(event.getArtist()).isEqualTo("Integration Artist");
    }
}
