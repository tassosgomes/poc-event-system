package com.music.service.controller;

import com.music.service.dto.SongRequest;
import com.music.service.dto.SongResponse;
import com.music.service.service.SongService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/songs")
@RequiredArgsConstructor
public class SongController {

    private final SongService songService;

    @PostMapping
    public ResponseEntity<SongResponse> createSong(@Valid @RequestBody SongRequest request) {
        SongResponse response = songService.createSong(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(response);
    }

    @GetMapping
    public ResponseEntity<List<SongResponse>> listSongs() {
        return ResponseEntity.ok(songService.listSongs());
    }
}
