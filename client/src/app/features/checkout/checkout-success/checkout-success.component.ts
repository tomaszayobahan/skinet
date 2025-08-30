import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, OnDestroy } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { AddressPipe } from '../../../shared/pipes/address-pipe';
import { CardPipe } from '../../../shared/pipes/card-pipe';
import { SignalrService } from '../../../core/services/signalr.service';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-checkout-success',
  imports: [
    MatButton,
    RouterLink,
    MatProgressSpinnerModule,
    DatePipe,
    AddressPipe,
    CurrencyPipe,
    CardPipe
],
  templateUrl: './checkout-success.component.html',
  styleUrl: './checkout-success.component.scss'
})
export class CheckoutSuccessComponent implements OnDestroy{
  signalrService = inject(SignalrService);
  private orderService = inject(OrderService);

  ngOnDestroy(): void {
      this.orderService.orderComplete = false;
      this.signalrService.orderSignal.set(null);
  }
}
