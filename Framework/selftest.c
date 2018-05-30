/*
 * selftest.c
 *
 * Created: 2018/3/6 9:34:48
 *  Author: A41536
 */ 
#include "selftest.h"
#include "atmel_start.h"
#include "usart_basic.h"
#include "datastreamer.h"
#include "driver_init.h"
#include "touch.h"
//#include "tca.h"

extern qtm_touch_key_group_data_t qtlib_key_grp_data_set1;
extern qtm_touch_key_control_t   qtlib_key_set1;
//extern five_tap_disable_timer;
//extern five_tap_disable;

bool get_sensor_status_command = false;
bool get_sensor_cc_val_command = false;
bool get_sensor_reference_command = false;
bool get_sensor_delta_command = false;
bool touch_is_sleep = false;
bool touch_is_awake = true;

//void RTC_set_period(const uint16_t val);

uint8_t USART_recv(void)
{
	return USART_read();
}

void uart_transmit(const uint8_t data)
{
	USART_write(data);
}

uint8_t crc8(uint8_t crc, uint8_t val)
{
	uint8_t fb;
	const uint8_t CRC_PLOY = 0x8c;
	
	for(int i = 0; i < 8; i++)
	{
		fb = (crc ^ val) & 0x01;
        val >>= 1;
        crc >>= 1;
        if (fb)
			crc ^= CRC_PLOY;
	}

    return crc;
}

uint8_t calculate_crc(const uint8_t *data, int len)
{
	uint8_t crc = 0;

    for(int i = 0; i < len; i++)
		crc = crc8(crc, data[i]);

	return crc;
}

void send_frame(uint8_t cmd, const uint8_t * data, uint8_t len)
{
	uint8_t i;
	uint8_t crc;
	uint8_t header[4] = {0xa5, 0x5a, 0, 0};
	
	//transfer head
	header[2] = cmd;
	header[3] = len;
	for(i = 0; i < sizeof(header); i++)
		uart_transmit(header[i]);
	
	crc = calculate_crc(header, sizeof(header));
	uart_transmit(crc);

	//transfer body
	for(i = 0; i < len; i++)
		uart_transmit(data[i]);
	
	crc = calculate_crc(data, len);
	uart_transmit(crc);
}

void response3(uint8_t cmd, uint8_t param, uint16_t data)
{
	uint8_t buf[3] = {param, (uint8_t)data, (uint8_t)(data >> 8u)};
	
	send_frame(cmd, buf, sizeof(buf));
}

void self_test_process(void)
{
	unsigned char command;
	unsigned char parameters;
	int16_t	temp_int_calc;
	uint16_t u16temp_output;
	
	if((get_sensor_cc_val_command ==  false) && (get_sensor_reference_command ==  false) && (get_sensor_delta_command == false) && (get_sensor_status_command == false))
	{
		if(USART_is_rx_ready() == true)
		{
			command = USART_recv();
			if(command == Selftest_Get_Sensor_CC_Val)
			{
				get_sensor_cc_val_command = true;
			}
			else if(command == Selftest_Get_Sensor_Reference)
			{
				get_sensor_reference_command = true;
			}
			else if(command == Selftest_Get_Sensor_Delta)
			{
				get_sensor_delta_command = true;
			}
			else if(command == Selftest_Get_Status)
			{
				get_sensor_status_command = true;
			}
		}
	}

	if(get_sensor_cc_val_command == true)
	{
		if(USART_is_rx_ready() == true)
		{
			parameters = USART_recv();
			u16temp_output = get_sensor_cc_val(parameters);
			/*
			uart_transmit(Selftest_Get_Sensor_CC_Val);
			uart_transmit(parameters);
			uart_transmit((uint8_t)u16temp_output);
			uart_transmit((uint8_t)(u16temp_output >> 8u));
			*/
			response3(Selftest_Get_Sensor_CC_Val, parameters, u16temp_output);
			get_sensor_cc_val_command = false;
		}
	}

	if(get_sensor_reference_command == true)
	{
		if(USART_is_rx_ready() == true)
		{
			parameters = USART_recv();
			/* Reference */
			u16temp_output = get_sensor_node_reference(parameters);
			/*
			uart_transmit(Selftest_Get_Sensor_Reference);
			uart_transmit(parameters);
			uart_transmit((uint8_t)u16temp_output);
			uart_transmit((uint8_t)(u16temp_output >> 8u));
			*/
			response3(Selftest_Get_Sensor_Reference, parameters, u16temp_output);
			get_sensor_reference_command = false;
		}
	}

	if(get_sensor_delta_command == true)
	{
		if(USART_is_rx_ready() == true)
		{
			parameters = USART_recv();
			/* Touch delta */
			temp_int_calc = get_sensor_node_signal(parameters);
			temp_int_calc -= get_sensor_node_reference(parameters);
			u16temp_output = (uint16_t)(temp_int_calc);
			/*
			uart_transmit(Selftest_Get_Sensor_Delta);
			uart_transmit(parameters);
			uart_transmit((uint8_t)u16temp_output);
			uart_transmit((uint8_t)(u16temp_output >> 8u));
			*/
			response3(Selftest_Get_Sensor_Delta, parameters, u16temp_output);
			get_sensor_delta_command = false;
		}
	}
	/*
	if(get_sensor_status_command == true)
	{
		if(USART_is_rx_ready() == true)
		{
			parameters = USART_recv();
			// Touch delta 
			u16temp_output = (uint16_t)(temp_int_calc);
			uart_transmit(Selftest_Get_Status);
			uart_transmit(parameters);
			if(parameters == 0x00)
			{
				uart_transmit((uint8_t) FW_Version);
				uart_transmit((uint8_t) (FW_Version >> 8u));
			}
			else if(parameters == 0x01)
			{
				uart_transmit((uint8_t) Selftest_OK);
				uart_transmit((uint8_t)(Selftest_OK >> 8u));
			}
			else if(parameters == 0x02)
			{
				uart_transmit((uint8_t) Sleep_Message);
				uart_transmit((uint8_t)(Sleep_Message >> 8u));
				RTC_set_period(2000);
				TIMER_0_stop();
				for(int i = 0; i < DEF_NUM_CHANNELS; i++)
				{
					qtm_key_suspend(i, &qtlib_key_set1);
				}
				touch_is_sleep = true;
				touch_is_awake = false;
				for(int j = 0; j < 1000; j++)
				{
				}
				sleep(PM_SLEEP_STANDBY);
			}
			else if(parameters == 0x03)
			{
				uart_transmit((uint8_t) Wake_Message);
				uart_transmit((uint8_t)(Wake_Message >> 8u));
				RTC_set_period(DEF_TOUCH_MEASUREMENT_PERIOD_MS);
				TIMER_0_init();
				for(int i = 0; i < DEF_NUM_CHANNELS; i++)
				{
					qtm_key_resume(i, &qtlib_key_set1);
				}
				touch_is_sleep = false;
				touch_is_awake = true;
			}
			else if(parameters == 0x04)
			{
				uart_transmit((uint8_t) Enable_Message);
				uart_transmit((uint8_t)(Enable_Message >> 8u));
				five_tap_disable_timer = 0;
				five_tap_disable = 0;
				
			}
			get_sensor_status_command = false;
		}
	}
	*/
}

#if 0
void touch_wakeup(void)
{
	uart_transmit((uint8_t) Wake_Message);
	uart_transmit((uint8_t)(Wake_Message >> 8u));
	RTC_set_period(DEF_TOUCH_MEASUREMENT_PERIOD_MS);
	TIMER_0_init();
	for(int i = 0; i < DEF_NUM_CHANNELS; i++)
	{
		qtm_key_resume(i, &qtlib_key_set1);
	}
	touch_is_sleep = false;
	touch_is_awake = true;
}

/*============================================================================
void Timer_set_period(const uint8_t val)
------------------------------------------------------------------------------
Purpose: This function sets the time interval on the RTC/Timer peripheral based
         on the user configuration.
Input  : Time interval
Output : none
Notes  :
============================================================================*/
void RTC_set_period(const uint16_t val)
{
	while (RTC.STATUS & RTC_PERBUSY_bm) /* wait for RTC synchronization */
		;
	RTC.PER = val;
}

#endif