package com.music.service.service;

import com.music.service.domain.Song;
import com.music.service.domain.SongStatus;
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
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
class SongServiceTest {

    @Mock
    private SongRepository songRepository;

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
}
