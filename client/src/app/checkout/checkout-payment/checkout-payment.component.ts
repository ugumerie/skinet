import { Component, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { IOrder, IOrderToCreate } from '../../shared/models/order';
import { BasketService } from '../../basket/basket.service';
import { IBasket } from '../../shared/models/basket';
import { CheckoutService } from '../checkout.service';
import { NavigationExtras, Router } from '@angular/router';

@Component({
  selector: 'app-checkout-payment',
  templateUrl: './checkout-payment.component.html',
  styleUrls: ['./checkout-payment.component.scss'],
})
export class CheckoutPaymentComponent implements OnInit {
  @Input() checkoutForm: FormGroup;

  constructor(
    private basketService: BasketService,
    private checkoutService: CheckoutService,
    private toastr: ToastrService,
    private router: Router
  ) {}

  ngOnInit(): void {}

  submitOrder(): void {
    const basket = this.basketService.getCurrentBasketValue();
    const orderToCreate = this.getOrderToCreate(basket);
    this.checkoutService.createOrder(orderToCreate).subscribe(
      (order: IOrder) => {
        this.toastr.success('Order created successfully', 'Done');
        this.basketService.deleteLocalBasket(basket.id);

        const navigationExtras: NavigationExtras = {state: order};
        this.router.navigate(['checkout/success'], navigationExtras);
      },
      (error) => {
        this.toastr.error(error.message, 'Error');
        console.log(error);
      }
    );
  }

  private getOrderToCreate(basket: IBasket): IOrderToCreate {
    return {
      basketId: basket.id,
      deliveryMethodId: +this.checkoutForm
        .get('deliveryForm')
        .get('deliveryMethod').value,
      shipToAddress: this.checkoutForm.get('addressForm').value,
    };
  }
}
