#pragma once

#include <led_strip.h>

#define LED_STRIP_LEN 144
#define AMBUFFER_SIZE 26

typedef struct
{
    uint8_t r;
    uint8_t g;
    uint8_t b;
}  RGBCol;

// uint8_t colorsSerial[26 * 3];
RGBCol colorsSerial[AMBUFFER_SIZE];
RGBCol colorsCurrent[AMBUFFER_SIZE];

led_strip_t *strip;
led_strip_t *strip2;


void led_task(void *pvParameters);