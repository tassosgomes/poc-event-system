package com.music.service.domain.event;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDate;
import java.util.UUID;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class SongCreatedEvent {
    private UUID songId;
    private String title;
    private String artist;
    private LocalDate releaseDate;
    private String genre;
}
