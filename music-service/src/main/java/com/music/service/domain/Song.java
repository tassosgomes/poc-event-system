package com.music.service.domain;

import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDate;
import java.util.UUID;

@Entity
@Table(name = "songs")
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class Song {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(nullable = false)
    private String title;

    @Column(nullable = false)
    private String artist;

    @Column(name = "release_date")
    private LocalDate releaseDate;

    private String genre;

    @Column(name = "video_url")
    private String videoUrl;

    private Long views;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false)
    private SongStatus status;
}
