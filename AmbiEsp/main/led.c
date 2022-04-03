#include "led.h"
#include <esp_log.h>
#include "driver/rmt.h"
#include <math.h>

#define MIN(a,b) ((a) < (b) ? (a) : (b))

void led_fill(uint8_t index, const RGBCol *rgb)
{
    strip->set_pixel(strip, LED_STRIP_LEN - index - 1, rgb->r, rgb->g, rgb->b);
}

const float stepSize = 5.5f;
void indexToColorIndexAndPartsParts(uint8_t i1, uint8_t *c1, uint8_t *c2, float *p1, float *p2)
{
    *c1 = (int)(i1 / stepSize);
    *c2 = *c1 + 1;
    if (*c2 >= AMBUFFER_SIZE)
    {
        *c2 = 0;
    }

    *p1 = fmod(i1, stepSize) / stepSize;
    *p2 = 1.0f - *p1;
}

void interpolate(const RGBCol *c1, const RGBCol *c2, float p1, float p2, RGBCol *col)
{
    col->r = (uint8_t)(c1->r * p1 + c2->r * p2);
    col->g = (uint8_t)(c1->g * p1 + c2->g * p2);
    col->b = (uint8_t)(c1->b * p1 + c2->b * p2);
}

void updateColor(uint8_t *is, uint8_t *should)
{
    const uint8_t MAX_STEP = 10;
    if(*is > *should)
        *is -= MIN(MAX_STEP, *is - *should);
    else
        *is += MIN(MAX_STEP, *should - *is);
}

void updateColorsCurrent()
{
    for (size_t i = 0; i < AMBUFFER_SIZE; i++)
    {
        updateColor(&colorsCurrent[i].r, &colorsSerial[i].r);
        updateColor(&colorsCurrent[i].g, &colorsSerial[i].g);
        updateColor(&colorsCurrent[i].b, &colorsSerial[i].b);
    }
}

void led_strip_fill_serial()
{
    uint8_t i1, i2;
    float p1, p2;
    RGBCol col;

    updateColorsCurrent();

    for (size_t i = 0; i < LED_STRIP_LEN; i++)
    {
        indexToColorIndexAndPartsParts(i, &i1, &i2, &p1, &p2);
        interpolate(&colorsCurrent[i1], &colorsCurrent[i2], p1, p2, &col);

        led_fill(i, &col);
    }
}

void led_task(void *pvParameters)
{
    ESP_LOGE("LED", "LED TASK STARTING");

    rmt_config_t config = RMT_DEFAULT_CONFIG_TX(18, RMT_CHANNEL_0);
    config.clk_div = 2;
    rmt_config(&config);
    rmt_driver_install(config.channel, 0, 0);

    led_strip_config_t strip_conf = LED_STRIP_DEFAULT_CONFIG(LED_STRIP_LEN, (led_strip_dev_t)config.channel);
    strip = led_strip_new_rmt_ws2812(&strip_conf);
    strip->clear(strip, 100);

    while (1)
    {
        led_strip_fill_serial(strip, strip2);

        strip->refresh(strip, 100);

        vTaskDelay(pdMS_TO_TICKS(50));
    }
}