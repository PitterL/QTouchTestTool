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

//mg0, no, dat0, dat1, dat3, dat4, dat5, crc
void send_frame(uint8_t seq, uint8_t dat0, uint8_t dat1, bool result)
{
	uint8_t i;
	uint8_t crc;
	uint8_t buf[8];
	
	if (result)
		buf[0] = 0x5a;  //magic word
	else
		buf[0] = 0xfe;	//failed word
		
	buf[1] = seq;
	buf[2] = dat0;
	buf[3] = dat1;
	//buf[4:6] random data
	
	crc = calculate_crc(buf, sizeof(buf) -1);
	buf[sizeof(buf) - 1] = crc;
	
	for(i = 0; i < sizeof(buf); i++)
		uart_transmit(buf[i]);
}

uint8_t recv_frame(uint8_t * data, uint8_t len, volatile int32_t timeout)
{
	uint8_t count = 0;
	
	while(timeout-- > 0)
	{
		if(USART_is_rx_ready()) {
			data[count++] = USART_read();
			
			if (count == len)
				break;
		}
	}
	
	return count;
}

void clear_frame()
{
	while(USART_is_rx_ready())
	{
		USART_read();
	}
}

void response(uint8_t seq, uint16_t data, bool result)
{
	send_frame(seq, (uint8_t)data, (uint8_t)(data >> 8u), result);
}

bool receive(uint8_t *data, uint8_t len)
{
	volatile int32_t timeout = 300000;
	uint8_t crc_calc;
	uint8_t count;
	
	count = recv_frame(data, len, timeout);
	if (count == len)
	{
		crc_calc = calculate_crc(data, len - 1);
		if (crc_calc == data[len - 1])
			return true;
	}
	
	data[len -1] = count;	//for debug use
	return false;
}


void self_test_process(void)
{
	int16_t	temp_int_calc;
	uint16_t u16temp_output;
	
	uint8_t buf[4];
	uint8_t seq, cmd, id;
	bool result = false;
	
	if (USART_is_rx_ready())
	{
		if (receive(buf, sizeof(buf)))
		{
			seq = buf[0];
			cmd = buf[1];
			id = buf[2];
			
			if (cmd == Selftest_Get_Sensor_CC_Val)
			{
				u16temp_output = get_sensor_cc_val(id);
				result = true;
			}
			else if(cmd == Selftest_Get_Sensor_Reference)
			{
				u16temp_output = get_sensor_node_reference(id);
				result = true;
			}
			else if(cmd == Selftest_Get_Sensor_Delta)
			{
				temp_int_calc = get_sensor_node_signal(id);
				temp_int_calc -= get_sensor_node_reference(id);
				u16temp_output = (uint16_t)(temp_int_calc);
				result = true;
			}
			else
			{
				u16temp_output = 0xfefe;
				clear_frame();
			}
		}else{
			u16temp_output = 0xffff;
			clear_frame();
		}
		
		response(seq, u16temp_output, result);	
	}
}
