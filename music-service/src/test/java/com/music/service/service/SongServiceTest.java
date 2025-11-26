package com.music.service.service;

import com.music.service.domain.Song;
import com.music.service.domain.SongStatus;
import com.music.service.domain.event.VideoFoundEvent;
import com.music.service.dto.SongRequest;
import com.music.service.dto.SongResponse;
import com.music.service.repository.SongRepository;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.time.LocalDate;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class SongServiceTest {

    @Mock
    private SongRepository songRepository;

    @Mock
    private SongEventPublisher songEventPublisher;

    @Mock
    private SongStreamService songStreamService;

    @InjectMocks
    private SongService songService;

    @Test
    void createSong_ShouldReturnResponse_WhenSuccessful() {
        SongRequest request = new SongRequest();
        request.setTitle("Test Song");
        request.setArtist("Test Artist");
        request.setReleaseDate(LocalDate.now());
        request.setGenre("Rock");

        Song savedSong = Song.builder()
                .id(UUID.randomUUID())
                .title("Test Song")
                .artist("Test Artist")
                .releaseDate(LocalDate.now())
                .genre("Rock")
                .status(SongStatus.PENDING)
                .build();

        when(songRepository.save(any(Song.class))).thenReturn(savedSong);

        SongResponse response = songService.createSong(request);

        assertNotNull(response);
        assertEquals(savedSong.getId(), response.getId());
        assertEquals(SongStatus.PENDING, response.getStatus());
        assertEquals("Test Song", response.getTitle());
    }

    @Test
    void listSongs_ShouldReturnList_WhenSongsExist() {
        Song song = Song.builder()
                .id(UUID.randomUUID())
                .title("Test Song")
                .artist("Test Artist")
                .status(SongStatus.PENDING)
                .build();

        when(songRepository.findAll()).thenReturn(List.of(song));

        List<SongResponse> responses = songService.listSongs();

        assertFalse(responses.isEmpty());
        assertEquals(1, responses.size());
    }

    @Test
    void processVideoFoundEvent_ShouldUpdateSongAndNotify() {
        UUID songId = UUID.randomUUID();
        Song existingSong = Song.builder()
                .id(songId)
                .title("Song")
                .artist("Artist")
                .status(SongStatus.PENDING)
                .build();

        VideoFoundEvent event = VideoFoundEvent.builder()
                .songId(songId)
                .videoUrl("https://youtube.com/watch?v=123")
                .views(1000L)
                .build();

        when(songRepository.findById(songId)).thenReturn(Optional.of(existingSong));
        when(songRepository.save(existingSong)).thenReturn(existingSong);

        songService.processVideoFoundEvent(event);

        assertEquals("https://youtube.com/watch?v=123", existingSong.getVideoUrl());
        assertEquals(1000L, existingSong.getViews());
        assertEquals(SongStatus.COMPLETED, existingSong.getStatus());
        verify(songRepository).save(existingSong);
        verify(songStreamService).broadcastSongUpdate(any(SongResponse.class));
    }

    @Test
    void processVideoFoundEvent_ShouldIgnoreWhenSongDoesNotExist() {
        UUID songId = UUID.randomUUID();
        VideoFoundEvent event = VideoFoundEvent.builder()
                .songId(songId)
                .videoUrl("url")
                .views(10L)
                .build();

        when(songRepository.findById(songId)).thenReturn(Optional.empty());

        songService.processVideoFoundEvent(event);

        verify(songRepository, never()).save(any(Song.class));
        verifyNoInteractions(songStreamService);
    }
}
