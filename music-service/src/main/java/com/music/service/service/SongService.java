package com.music.service.service;

import com.music.service.domain.Song;
import com.music.service.domain.SongStatus;
import com.music.service.domain.event.SongCreatedEvent;
import com.music.service.domain.event.VideoFoundEvent;
import com.music.service.dto.SongRequest;
import com.music.service.dto.SongResponse;
import com.music.service.repository.SongRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
@Slf4j
public class SongService {

    private final SongRepository songRepository;
    private final SongEventPublisher songEventPublisher;
    private final SongStreamService songStreamService;

    @Transactional
    public SongResponse createSong(SongRequest request) {
        Song song = Song.builder()
                .title(request.getTitle())
                .artist(request.getArtist())
                .releaseDate(request.getReleaseDate())
                .genre(request.getGenre())
                .status(SongStatus.PENDING)
                .build();

        Song savedSong = songRepository.save(song);

        SongCreatedEvent event = SongCreatedEvent.builder()
                .songId(savedSong.getId())
                .title(savedSong.getTitle())
                .artist(savedSong.getArtist())
                .releaseDate(savedSong.getReleaseDate())
                .genre(savedSong.getGenre())
                .build();
        
        songEventPublisher.publishSongCreatedEvent(event);

        return mapToResponse(savedSong);
    }

    @Transactional(readOnly = true)
    public List<SongResponse> listSongs() {
        return songRepository.findAll().stream()
                .map(this::mapToResponse)
                .collect(Collectors.toList());
    }

    @Transactional
    public void processVideoFoundEvent(VideoFoundEvent event) {
        songRepository.findById(event.getSongId()).ifPresentOrElse(song -> {
            song.setVideoUrl(event.getVideoUrl());
            song.setViews(event.getViews());
            song.setStatus(SongStatus.COMPLETED);

            Song updatedSong = songRepository.save(song);
            songStreamService.broadcastSongUpdate(mapToResponse(updatedSong));
            log.info("Song {} updated after VideoFoundEvent", song.getId());
        }, () -> log.warn("Received VideoFoundEvent for unknown song {}", event.getSongId()));
    }

    private SongResponse mapToResponse(Song song) {
        return SongResponse.builder()
                .id(song.getId())
                .title(song.getTitle())
                .artist(song.getArtist())
                .releaseDate(song.getReleaseDate())
                .genre(song.getGenre())
                .videoUrl(song.getVideoUrl())
                .views(song.getViews())
                .status(song.getStatus())
                .build();
    }
}
