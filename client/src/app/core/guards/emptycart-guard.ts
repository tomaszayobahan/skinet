import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { CartService } from '../services/cart.service';
import { SnackbarService } from '../services/snackbar.service';

export const emptycartGuard: CanActivateFn = (route, state) => {
  const cartService = inject(CartService);
  const snackbar = inject(SnackbarService);

  if (!cartService.itemCount()) { // could error because not cartService.cart()? (probably not)
    snackbar.error('Your cart is empty');
    return false;
  }

  return true;
};
