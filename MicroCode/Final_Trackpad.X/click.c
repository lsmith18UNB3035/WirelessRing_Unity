#pragma config ALTI2C1 = ON
// FOSCSEL
#pragma config FNOSC = FRC              // Oscillator Source Selection (Primary Oscillator (XT, HS, EC))
#pragma config IESO = ON                // Two-speed Oscillator Start-up Enable bit (Start up device with FRC, then switch to user-selected oscillator source)
// FOSC
#pragma config POSCMD = HS              // Primary Oscillator Mode Select bits (HS Crystal Oscillator Mode)
#pragma config OSCIOFNC = OFF           // OSC2 Pin Function bit (OSC2 is clock output)
#pragma config FCKSM = CSDCMD           // Clock Switching Mode bits (Both Clock switching and Fail-safe Clock Monitor are disabled)
#pragma config PLLKEN = ON              // PLL Lock Status Control (PLL lock signal will be used to disable PLL clock output if lock is lost)
#pragma config XTCFG = G3               // XT Config (24-32 MHz crystals)
#pragma config XTBST = ENABLE           // XT Boost (Boost the kick-start)

#pragma config ICS = 2

#define FCY 4000000UL
#define FP 4000000
#define BAUDRATE 9600
#define BRGVAL ((FP/BAUDRATE)/16)-1
#define DELAY_105uS asm volatile ("REPEAT, #4201"); Nop();

#include <libpic30.h>
#include "xc.h"
#include "time.h"
#include "stdio.h"

char deltaX;
char deltaY;
char motreg;
int mot;
char click;

void I2C_Write(uint8_t I2CAdd, uint8_t RegAdd, uint8_t data);
char I2C_Read(uint8_t I2CAdd, uint8_t RegAdd);

// **TX interrupt //
void __attribute__((__interrupt__)) _U1TXInterrupt(void){
    IFS0bits.U1TXIF = 0;
    __delay_ms(10); 
    //}
}

int main(void) {  
    // set up board
    TRISDbits.TRISD10 = 1;      // button input
    ANSELDbits.ANSELD10 = 0;    // digital button
    TRISBbits.TRISB14 = 1;      // Set RX as input
    TRISBbits.TRISB13 = 0;      // Set TX as output
    
    // Assign RX and TX pins for UNB Dev Board
    RPCONbits.IOLOCK = 0;       // All Peripheral Remapping registers are unlocked and can be written
    RPINR18bits.U1RXR = 46;     // Assign UART1 Receive (U1RX) to RP46 pin (BM_TX)
    RPOR6bits.RP45R = 1;        // Assign RP45 (BM_RX) output pin to UART1 Transmit (U1TX)
    RPCONbits.IOLOCK = 1;       // All Peripheral Remapping registers are locked and cannot be written
    
    // UART Setup
    U1BRG = BRGVAL;             
    // UART interrupt
    INTCON2bits.GIE = 1;        // Global Interrupt Enabled
    IEC0bits.U1TXIE = 1;        // Enable UART1 Transmitter Interrupt
    // UART Transmit
    U1MODEbits.MOD = 0;         // Asynchronous 8-bit UART
    U1MODEHbits.UTXINV = 0;     // Output data are not inverted; TX output is high in Idle state
    U1MODEHbits.STSEL = 0;      // 1 Stop bit sent, 1 checked at receive
    U1MODEHbits.FLO = 0;        // Flow control off
    U1STAHbits.UTXISEL = 0;     // Triggers transmit interrupt when there are eight empty slots in the buffer; TX buffer is empty
    U1STAHbits.URXISEL = 0;     // Trigger interrupt when one word waiting in the buffer
    U1MODEbits.UARTEN = 1;      // UART is ready to transmit and receive
    U1MODEbits.UTXEN = 1;       // Transmit enabled
    U1MODEbits.URXEN = 1;       //Receive Enabled
    
    I2C1CONLbits.I2CEN = 0;  // Disable I2C
    //Now, configure the I2C 1 module the way I want.
    I2C1BRG = 18;               // Should yield ~100 kHz clock frequency.
    I2C1CONLbits.I2CEN = 1;     // Enable the I2C module.
    __delay_ms(1);              // A 1 ms delay to allow the peripheral to start up. 
    
    DELAY_105uS     
    U1TXREG = 'a';
    __delay_ms(20); 

    I2C_Write(0x53,0x05,0b00000100);        // config byte
    I2C_Write(0x53,0x05,0b00100100);        // config byte
//    I2C_Write(0x53,0x0A,0x00);   
//    I2C_Write(0x53,0x0B,0x0b00000110);     
//    I2C_Write(0x53,0x0E,0b0100101);  
//    I2C_Write(0x53,0x0F,0b00000100);
//    I2C_Write(0x53,0x27,0b00011011);
//    I2C_Write(0x53,0x29,0x04); 
//    I2C_Write(0x53,0x41,0x80);  
//    I2C_Write(0x53,0x43,0b00010010); 
//    I2C_Write(0x53,0x54,0); 
//    I2C_Write(0x53,0x82,0b00000010); 
    I2C_Write(0x53,0x2A,0x40);              // x resolution
    I2C_Write(0x53,0x2B,0x40);              // y resolution
    
    I2C_Write(0x53,0x05,0b10101100);        // config byte
    
    while(1){
//        I2C_Read(0x53,0x31);
//        I2C_Read(0x53,0x32);
//        I2C_Read(0x53,0x0E);
//        I2C_Read(0x53,0x0F);
    motreg = I2C_Read(0x53,0x23);
    deltaX = I2C_Read(0x53,0x21);
        deltaY = I2C_Read(0x53,0x22); 
    if (motreg & (1<<2))
        mot = 1;
    else
        mot = 0;
    if(mot){
        if(deltaX == 0x01)
            deltaX = 0x00;
        if(deltaY == 0x01)
            deltaY = 0x02;
        TXDelta(deltaX,deltaY);
    }
        if(PORTDbits.RD10 == 1){
            click = 0x01;
            U1TXREG = click;
            __delay_ms(150);
        }
    } 
    return 0;
}

int I2C_Start() {
    I2C1STATbits.ACKSTAT = 1; // reset ack
    I2C1CONLbits.SEN = 1;   // I2C start sequence
    while(I2C1CONLbits.SEN);
    return 1;
}

void I2C_Stop() {
    I2C1CONLbits.RCEN = 0;
    I2C1CONLbits.PEN = 1; 
    while(I2C1CONLbits.PEN);
    __delay_ms(1);
}

void I2C_Restart(void)
{
    I2C1CONLbits.RSEN = 1;       // Repeated Start Condition
    while(I2C1CONLbits.RSEN);
    __delay_ms(1);
    //I2C1CONLbits.ACKDT = 0;      // Send ACK
    //I2C1STATbits.TBF = 0;       // I2C1TRN is empty
}

void I2C_Write(uint8_t I2CAdd, uint8_t RegAdd, uint8_t data) {
    I2C_Start();
    I2C1TRNbits.I2CTXDATA = (I2CAdd << 1) & 0xFE;
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
  
    I2C1TRNbits.I2CTXDATA = (RegAdd);
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
    
    I2C1TRNbits.I2CTXDATA = (data);
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
    I2C_Stop();
}

char I2C_Read(uint8_t I2CAdd, uint8_t RegAdd) {
    char value;
    I2C_Start();
    I2C1TRNbits.I2CTXDATA = (I2CAdd << 1) & 0xFE;
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
    
    I2C1TRNbits.I2CTXDATA = (RegAdd);
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
    //I2C_Stop();
    I2C_Restart();
    I2C1TRNbits.I2CTXDATA = ((I2CAdd << 1) | 0x01);
    while(I2C1STATbits.ACKSTAT); //Wait for ack
    while(I2C1STATbits.TRSTAT); //wait until transaction complete
    
    I2C1CONLbits.RCEN = 1;
    while(!I2C1STATbits.RBF);
    while(I2C1CONLbits.RCEN); //the RCEN reg is cleared automatically
    //while(!(I2C1STATbits.ACKSTAT)); // Wait for nack
    I2C1CONLbits.ACKDT = 0x1;  //set the nack
    I2C1CONLbits.ACKEN = 0x1;  //set that you send an nack
    while(I2C1CONLbits.ACKEN); //waiting for it to clear
    I2C_Stop();
    value = I2C1RCV;
    return value;
}

void TXDelta(uint8_t x, uint8_t y){
    __delay_ms(20);
    U1TXREG = x;
    __delay_ms(20); 
    U1TXREG = y;
    __delay_ms(20); 
}
