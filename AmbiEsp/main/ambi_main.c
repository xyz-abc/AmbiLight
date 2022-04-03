/* UART Echo Example

   This example code is in the Public Domain (or CC0 licensed, at your option.)

   Unless required by applicable law or agreed to in writing, this
   software is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
   CONDITIONS OF ANY KIND, either express or implied.
*/
#include <stdio.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "driver/uart.h"
#include "driver/gpio.h"
#include "sdkconfig.h"
#include "led.h"
#include "string.h"
#include <esp_log.h>

/**
 * This is an example which echos any data it receives on configured UART back to the sender,
 * with hardware flow control turned off. It does not use UART driver event queue.
 *
 * - Port: configured UART
 * - Receive (Rx) buffer: on
 * - Transmit (Tx) buffer: off
 * - Flow control: off
 * - Event queue: off
 * - Pin assignment: see defines below (See Kconfig)
 */

#define BUF_SIZE (1024)
#define STACK_SIZE (2048)


static void uart_task(void *arg)
{
    uart_config_t uart_config = {
        .baud_rate = CONFIG_AMBI_UART_SPEED,
        .data_bits = UART_DATA_8_BITS,
        .parity = UART_PARITY_DISABLE,
        .stop_bits = UART_STOP_BITS_1,
        .flow_ctrl = UART_HW_FLOWCTRL_DISABLE,
        .source_clk = UART_SCLK_APB,
    };
    int intr_alloc_flags = 0;

#if CONFIG_UART_ISR_IN_IRAM
    intr_alloc_flags = ESP_INTR_FLAG_IRAM;
#endif

    ESP_ERROR_CHECK(uart_driver_install(ECHO_UART_PCONFIG_AMBI_UART_PORT_NUMORT_NUM, BUF_SIZE * 2, 0, 0, NULL, intr_alloc_flags));
    ESP_ERROR_CHECK(uart_param_config(CONFIG_AMBI_UART_PORT_NUM, &uart_config));
    ESP_ERROR_CHECK(uart_set_pin(CONFIG_AMBI_UART_PORT_NUM, CONFIG_AMBI_UART_TXD, CONFIG_AMBI_UART_RX, UART_PIN_NO_CHANGE, UART_PIN_NO_CHANGE));

    // Configure a temporary buffer for the incoming data
    uint8_t *data = (uint8_t *)malloc(BUF_SIZE);
    size_t availLen = 0;

    while (1)
    {
        uart_get_buffered_data_len(ECHO_UART_PORT_NUM, &availLen);
        if (availLen >= 78)
        {
            uart_read_bytes(ECHO_UART_PORT_NUM, data, BUF_SIZE, 20 / portTICK_RATE_MS);
            memcpy(colorsSerial, data, 78);
        }
    }
}

void app_main(void)
{
    xTaskCreatePinnedToCore(led_task, "led_task", STACK_SIZE * 5, NULL, 5, NULL, 1);
    xTaskCreate(uart_task, "uart_task", STACK_SIZE, NULL, 10, NULL);
}
