package com.music.service.config;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;
import org.springframework.amqp.core.*;
import org.springframework.amqp.rabbit.annotation.EnableRabbit;
import org.springframework.amqp.rabbit.connection.ConnectionFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.amqp.support.converter.Jackson2JsonMessageConverter;
import org.springframework.amqp.support.converter.MessageConverter;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
@EnableRabbit
public class RabbitMQConfig {

    @Value("${app.rabbitmq.exchange}")
    private String exchangeName;

    @Value("${app.rabbitmq.queue.song-created}")
    private String songCreatedQueueName;

    @Value("${app.rabbitmq.routing-key.song-created}")
    private String songCreatedRoutingKey;

    @Value("${app.rabbitmq.queue.video-found}")
    private String videoFoundQueueName;

    @Value("${app.rabbitmq.routing-key.video-found}")
    private String videoFoundRoutingKey;

    @Bean
    public TopicExchange exchange() {
        return new TopicExchange(exchangeName);
    }

    @Bean
    public Queue songCreatedQueue() {
        return QueueBuilder.durable(songCreatedQueueName).build();
    }

    @Bean
    public Queue videoFoundQueue() {
        return QueueBuilder.durable(videoFoundQueueName).build();
    }

    @Bean
    public Binding songCreatedBinding(@Qualifier("songCreatedQueue") Queue songCreatedQueue,
                                      TopicExchange exchange) {
        return BindingBuilder.bind(songCreatedQueue).to(exchange).with(songCreatedRoutingKey);
    }

    @Bean
    public Binding videoFoundBinding(@Qualifier("videoFoundQueue") Queue videoFoundQueue,
                                     TopicExchange exchange) {
        return BindingBuilder.bind(videoFoundQueue).to(exchange).with(videoFoundRoutingKey);
    }

    @Bean
    public MessageConverter jsonMessageConverter() {
        ObjectMapper objectMapper = new ObjectMapper();
        objectMapper.registerModule(new JavaTimeModule());
        return new Jackson2JsonMessageConverter(objectMapper);
    }

    @Bean
    public RabbitTemplate rabbitTemplate(ConnectionFactory connectionFactory) {
        RabbitTemplate rabbitTemplate = new RabbitTemplate(connectionFactory);
        rabbitTemplate.setMessageConverter(jsonMessageConverter());
        return rabbitTemplate;
    }
}
