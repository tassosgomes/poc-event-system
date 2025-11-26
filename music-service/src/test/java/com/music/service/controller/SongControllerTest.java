package com.music.service.controller;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.music.service.domain.SongStatus;
import com.music.service.dto.SongRequest;
import com.music.service.dto.SongResponse;
import com.music.service.service.SongService;
import com.music.service.service.SongStreamService;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.web.servlet.mvc.method.annotation.SseEmitter;

import java.time.LocalDate;
import java.util.List;
import java.util.UUID;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@WebMvcTest(SongController.class)
class SongControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @MockBean
    private SongService songService;

    @MockBean
    private SongStreamService songStreamService;

    @Autowired
    private ObjectMapper objectMapper;

    @Test
    void createSong_ShouldReturnCreated_WhenValidRequest() throws Exception {
        SongRequest request = new SongRequest();
        request.setTitle("Test Song");
        request.setArtist("Test Artist");
        request.setReleaseDate(LocalDate.now());

        SongResponse response = SongResponse.builder()
                .id(UUID.randomUUID())
                .title("Test Song")
                .artist("Test Artist")
                .status(SongStatus.PENDING)
                .build();

        when(songService.createSong(any(SongRequest.class))).thenReturn(response);

        mockMvc.perform(post("/songs")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isCreated())
                .andExpect(jsonPath("$.id").exists())
                .andExpect(jsonPath("$.title").value("Test Song"));
    }

    @Test
    void listSongs_ShouldReturnOk() throws Exception {
        when(songService.listSongs()).thenReturn(List.of());

        mockMvc.perform(get("/songs"))
                .andExpect(status().isOk());
    }

    @Test
    void streamSongs_ShouldReturnEmitter() throws Exception {
        when(songStreamService.registerEmitter()).thenReturn(new SseEmitter());

        mockMvc.perform(get("/songs/stream"))
                .andExpect(status().isOk());
    }
}
