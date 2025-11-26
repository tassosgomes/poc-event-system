package com.music.service.dto;

import com.music.service.domain.SongStatus;
import lombok.Builder;
import lombok.Data;

import java.time.LocalDate;
import java.util.UUID;

@Data
@Builder
public class SongResponse {
    private UUID id;
    private String title;
    private String artist;
    private LocalDate releaseDate;
    private String genre;
    private String videoUrl;
    private Long views;
    private SongStatus status;
}
